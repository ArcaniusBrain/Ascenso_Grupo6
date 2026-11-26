using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ascenso.Contracts.Eventos;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Donantes;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Presupuesto;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Programas;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Tiempo;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.IntegrationTests;

public sealed class PresupuestoCuposApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task HU12_1_Y_HU12_2_Consulta_Reservas_Renovaciones_Liberacion_Idempotencia_Y_Outbox()
    {
        await using var factory = new PresupuestoApiFactory();
        using var client = factory.CreateClient();

        // Preparar periodo, programa (cupo 2, duración 2, 600 USD/periodo => 1.200 USD por beca R32/A2)
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitudPublica(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")))).StatusCode);
        var programa = await (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Ingeniería", "2026-2", 2, 2, 600m, "USD", ModalidadPago.DIRECTO_UNIVERSIDAD, AlcancePrograma.COMPLETO))))
            .Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);

        // HU-12.1 E3: programa sin aportes -> total 0, disponible 0 y mensaje literal
        var rSinAportes = await client.SendAsync(CrearSolicitudPublica(HttpMethod.Get, $"/api/v1/programas/{programa!.Id}/presupuesto", "COORDINADOR"));
        Assert.Equal(HttpStatusCode.OK, rSinAportes.StatusCode);
        var sinAportes = await rSinAportes.Content.ReadFromJsonAsync<PresupuestoProgramaDto>(JsonOptions);
        Assert.Equal(0m, sinAportes!.PresupuestoTotal);
        Assert.Equal(0m, sinAportes.PresupuestoDisponible);
        Assert.Equal("Este programa no tiene aportes registrados.", sinAportes.MensajeSinAportes);

        // HU-12.1 E7 & HU-12.2 E5: Becario, Postulante, Evaluador y Enlace bloqueados -> 403
        foreach (var rolProhibido in new[] { "BECARIO", "POSTULANTE", "EVALUADOR", "ENLACE_UNIVERSIDAD" })
        {
            var r403 = await client.SendAsync(CrearSolicitudPublica(HttpMethod.Get, $"/api/v1/programas/{programa.Id}/presupuesto", rolProhibido));
            Assert.Equal(HttpStatusCode.Forbidden, r403.StatusCode);
        }

        // Registrar donante y aporte de 6.000 USD vía RF-13 -> sincroniza automáticamente con PresupuestoPrograma
        var donante = await (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            "/api/v1/donantes",
            "COORDINADOR",
            new RegistrarDonanteRequest("Empresa Alfa", TipoDonante.ORGANIZACION, new ContactoDonanteDto("contacto@alfa.org", null)))))
            .Content.ReadFromJsonAsync<DonanteResumenDto>(JsonOptions);

        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            $"/api/v1/donantes/{donante!.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteDesdeDonanteRequest(programa.Id, 6000m, "USD", "2026-2")))).StatusCode);

        // Finanzas consulta presupuesto (HU-12.1 E1 / HU-12.2 E4)
        var pInicial = await (await client.SendAsync(CrearSolicitudPublica(HttpMethod.Get, $"/api/v1/programas/{programa.Id}/presupuesto", "FINANZAS")))
            .Content.ReadFromJsonAsync<PresupuestoProgramaDto>(JsonOptions);
        Assert.Equal(6000m, pInicial!.PresupuestoTotal);
        Assert.Equal(6000m, pInicial.PresupuestoDisponible);
        Assert.Equal(0m, pInicial.ComprometidoRenovacionesPendientes);
        Assert.Null(pInicial.MensajeSinAportes);

        // HU-12.2 E1: S4 reserva renovación pendiente de 5.000 USD -> disponible baja a 1.000 USD
        var tokenReservar = CrearJwtServicio("ascenso-s4-becas", "fondos.reservar");
        var tokenLiberar = CrearJwtServicio("ascenso-s4-becas", "fondos.liberar");
        var tokenConsultar = CrearJwtServicio("ascenso-s3-postulacion", "fondos.consultar");

        var rRen = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            "/internal/v1/fondos/reservas/renovaciones",
            tokenReservar,
            new ReservaRenovacionInternaCommandRequest(programa.Id, Guid.NewGuid(), "2026-2", 5000m, "USD", Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.OK, rRen.StatusCode);
        var ren = await rRen.Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions);
        Assert.Equal("RESERVADA", ren!.Estado);
        Assert.Equal(5000m, ren.MontoCalculado);

        // HU-12.2 E2: intentar oferta nueva (600 * 2 = 1.200 USD) con 1.000 disponibles -> 409 mensaje literal
        var rInsuf = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            "/internal/v1/fondos/reservas",
            tokenReservar,
            new ReservaInternaCommandRequest(programa.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "2026-2")));
        Assert.Equal(HttpStatusCode.Conflict, rInsuf.StatusCode);
        Assert.Contains("Presupuesto insuficiente: disponible 1.000, la beca requiere 1.200.", await rInsuf.Content.ReadAsStringAsync());

        // HU-12.2 E3: rechazar renovación libera los 5.000 USD
        var rLibRen = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            $"/internal/v1/fondos/reservas/{ren.ReservaId}/liberar",
            tokenLiberar,
            new LiberacionInternaCommandRequest(Guid.NewGuid(), MotivoLiberacionReserva.RECHAZO)));
        Assert.Equal(HttpStatusCode.OK, rLibRen.StatusCode);
        Assert.Equal("LIBERADA", (await rLibRen.Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions))!.Estado);

        // HU-12.1 E4, E10 & R32/A2: reservar oferta 1 (1.200 USD, duración 2) + idempotencia por operationId + bloqueo duplicado (postulacion, ciclo)
        var post1 = Guid.NewGuid();
        var ciclo1 = Guid.NewGuid();
        var op1 = Guid.NewGuid();
        var rOf1 = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            "/internal/v1/fondos/reservas",
            tokenReservar,
            new ReservaInternaCommandRequest(programa.Id, post1, ciclo1, op1, "2026-2")));
        Assert.Equal(HttpStatusCode.OK, rOf1.StatusCode);
        var of1 = await rOf1.Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions);
        Assert.Equal(1200m, of1!.MontoCalculado);
        Assert.Equal(2, of1.DuracionPeriodos);

        var rOf1Rep = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            "/internal/v1/fondos/reservas",
            tokenReservar,
            new ReservaInternaCommandRequest(programa.Id, post1, ciclo1, op1, "2026-2")));
        Assert.Equal(HttpStatusCode.OK, rOf1Rep.StatusCode);
        Assert.Equal(of1.ReservaId, (await rOf1Rep.Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions))!.ReservaId);

        var rOf1Dup = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            "/internal/v1/fondos/reservas",
            tokenReservar,
            new ReservaInternaCommandRequest(programa.Id, post1, ciclo1, Guid.NewGuid(), "2026-2")));
        Assert.Equal(HttpStatusCode.Conflict, rOf1Dup.StatusCode);

        // S3 consulta disponibilidad autoritativa (scope fondos.consultar)
        var dispS3 = await (await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Get,
            $"/internal/v1/fondos/{programa.Id}/disponibilidad",
            tokenConsultar)))
            .Content.ReadFromJsonAsync<DisponibilidadInternaDto>(JsonOptions);
        Assert.Equal(4800m, dispS3!.Disponible);
        Assert.Equal(1, dispS3.CupoDisponible);

        // HU-12.1 E9: ocupar último cupo (oferta 2) y verificar bloqueo de oferta 3
        var rOf2 = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            "/internal/v1/fondos/reservas",
            tokenReservar,
            new ReservaInternaCommandRequest(programa.Id, Guid.NewGuid(), ciclo1, Guid.NewGuid(), "2026-2")));
        Assert.Equal(HttpStatusCode.OK, rOf2.StatusCode);
        var of2 = await rOf2.Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions);

        var rOf3SinCupo = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            "/internal/v1/fondos/reservas",
            tokenReservar,
            new ReservaInternaCommandRequest(programa.Id, Guid.NewGuid(), ciclo1, Guid.NewGuid(), "2026-2")));
        Assert.Equal(HttpStatusCode.Conflict, rOf3SinCupo.StatusCode);
        Assert.Contains("El programa no tiene cupo disponible.", await rOf3SinCupo.Content.ReadAsStringAsync());

        // Confirmar oferta 1 (otorgada; mantiene cupo y monto también si pasa a Inactiva, R27 / HU-12.1 E12)
        using (var scope = factory.Services.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ConfirmarReservaCommand(of1.ReservaId));
        }

        // HU-12.1 E2 + SEGURIDAD_INTERSERVICIOS prueba (f): liberar oferta 2 dos veces -> sin doble liberación y solo 1 evento Outbox
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            $"/internal/v1/fondos/reservas/{of2!.ReservaId}/liberar",
            tokenLiberar,
            new LiberacionInternaCommandRequest(Guid.NewGuid(), MotivoLiberacionReserva.RECHAZO)))).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            $"/internal/v1/fondos/reservas/{of2.ReservaId}/liberar",
            tokenLiberar,
            new LiberacionInternaCommandRequest(Guid.NewGuid(), MotivoLiberacionReserva.RECHAZO)))).StatusCode);

        var pFinal = await (await client.SendAsync(CrearSolicitudPublica(HttpMethod.Get, $"/api/v1/programas/{programa.Id}/presupuesto", "FINANZAS")))
            .Content.ReadFromJsonAsync<PresupuestoProgramaDto>(JsonOptions);
        Assert.Equal(1200m, pFinal!.TotalOtorgado);
        Assert.Equal(0m, pFinal.ComprometidoOfertasPendientes);
        Assert.Equal(4800m, pFinal.PresupuestoDisponible);
        Assert.Equal(1, pFinal.CupoOcupado);
        Assert.Equal(1, pFinal.CupoDisponible);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogoFondosDbContext>();
            var eventos = await db.OutboxMessages
                .Where(m => m.EventType == LiberarReservaInternaHandler.NombreEventoCupoLiberado)
                .ToListAsync();
            Assert.Single(eventos);
            var payload = JsonSerializer.Deserialize<CupoLiberadoV1>(eventos[0].PayloadJson, JsonOptions);
            Assert.Equal(programa.Id, payload!.ProgramaId);
            Assert.Equal(1200m, payload.MontoLiberado);
        }
    }

    [Fact]
    public async Task HU12_1_E6_Ultimo_Cupo_En_Disputa_Concurrencia_Solo_Uno_Confirma()
    {
        await using var factory = new PresupuestoApiFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitudPublica(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")))).StatusCode);
        var programa = await (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Cupo Unico", "2026-2", 1, 2, 1000m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.COMPLETO))))
            .Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);

        var donante = await (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            "/api/v1/donantes",
            "COORDINADOR",
            new RegistrarDonanteRequest("Fundación Beta", TipoDonante.ORGANIZACION, new ContactoDonanteDto("beta@org.com", null)))))
            .Content.ReadFromJsonAsync<DonanteResumenDto>(JsonOptions);

        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            $"/api/v1/donantes/{donante!.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteDesdeDonanteRequest(programa!.Id, 10000m, "USD", "2026-2")))).StatusCode);

        var token = CrearJwtServicio("ascenso-s4-becas", "fondos.reservar");
        var cicloId = Guid.NewGuid();
        var barrera = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<HttpResponseMessage> ReservarEnParalelo()
        {
            await barrera.Task;
            return await client.SendAsync(CrearSolicitudInterna(
                HttpMethod.Post,
                "/internal/v1/fondos/reservas",
                token,
                new ReservaInternaCommandRequest(programa.Id, Guid.NewGuid(), cicloId, Guid.NewGuid(), "2026-2")));
        }

        var t1 = ReservarEnParalelo();
        var t2 = ReservarEnParalelo();
        barrera.SetResult();
        var respuestas = await Task.WhenAll(t1, t2);

        Assert.Equal(1, respuestas.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(1, respuestas.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        var rechazada = respuestas.Single(r => r.StatusCode == HttpStatusCode.Conflict);
        var cuerpoRechazo = await rechazada.Content.ReadAsStringAsync();
        Assert.Contains("El programa no tiene cupo disponible.", cuerpoRechazo);

        var estadoFinal = await (await client.SendAsync(CrearSolicitudPublica(HttpMethod.Get, $"/api/v1/programas/{programa.Id}/presupuesto", "COORDINADOR")))
            .Content.ReadFromJsonAsync<PresupuestoProgramaDto>(JsonOptions);
        Assert.Equal(1, estadoFinal!.CupoOcupado);
        Assert.Equal(0, estadoFinal.CupoDisponible);
        Assert.Equal(2000m, estadoFinal.ComprometidoOfertasPendientes);
    }

    [Fact]
    public async Task Seguridad_Frontera_Privada_Rechaza_SinToken_UsuarioFinal_Y_ScopeIncorrecto()
    {
        await using var factory = new PresupuestoApiFactory();
        using var client = factory.CreateClient();

        // Sin token -> 401
        var r401 = await client.PostAsJsonAsync(
            "/internal/v1/fondos/reservas",
            new ReservaInternaCommandRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "2026-2"));
        Assert.Equal(HttpStatusCode.Unauthorized, r401.StatusCode);

        // Token de usuario final (postulante con email/role, incluso aunque traiga scope) -> 403
        var tokenPostulante = CrearJwtUsuarioFinal("postulante@ucab.edu.ve", "POSTULANTE", "fondos.reservar");
        var rUsuarioFinal = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            "/internal/v1/fondos/reservas",
            tokenPostulante,
            new ReservaInternaCommandRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "2026-2")));
        Assert.Equal(HttpStatusCode.Forbidden, rUsuarioFinal.StatusCode);

        // Token de servicio S3 (solo fondos.consultar) intentando liberar -> 403
        var tokenSoloConsulta = CrearJwtServicio("ascenso-s3-postulacion", "fondos.consultar");
        var rScopeInvalido = await client.SendAsync(CrearSolicitudInterna(
            HttpMethod.Post,
            $"/internal/v1/fondos/reservas/{Guid.NewGuid()}/liberar",
            tokenSoloConsulta,
            new LiberacionInternaCommandRequest(Guid.NewGuid(), MotivoLiberacionReserva.COMPENSACION)));
        Assert.Equal(HttpStatusCode.Forbidden, rScopeInvalido.StatusCode);
    }

    [Fact]
    public async Task ADR26_Conciliador_Gracia30Min_Vincula_Libera_Y_FailClosed()
    {
        var relojFijo = new RelojFijoPruebas(new DateTimeOffset(2026, 11, 26, 12, 0, 0, TimeSpan.Zero));
        var consultorStub = new ConsultorVinculoReservaS4Stub();

        await using var factory = new PresupuestoApiFactory(relojFijo, consultorStub);
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitudPublica(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")))).StatusCode);
        var programa = await (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Conciliacion", "2026-2", 5, 1, 1000m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL))))
            .Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);
        var donante = await (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            "/api/v1/donantes",
            "COORDINADOR",
            new RegistrarDonanteRequest("Donante Gamma", TipoDonante.PERSONA, new ContactoDonanteDto("gamma@correo.com", null)))))
            .Content.ReadFromJsonAsync<DonanteResumenDto>(JsonOptions);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitudPublica(
            HttpMethod.Post,
            $"/api/v1/donantes/{donante!.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteDesdeDonanteRequest(programa!.Id, 10000m, "USD", "2026-2")))).StatusCode);

        var tokenReservar = CrearJwtServicio("ascenso-s4-becas", "fondos.reservar");
        var cicloId = Guid.NewGuid();

        // 3 reservas antiguas (creadas a las 12:00 UTC)
        var rVinculable = (await (await client.SendAsync(CrearSolicitudInterna(HttpMethod.Post, "/internal/v1/fondos/reservas", tokenReservar, new ReservaInternaCommandRequest(programa.Id, Guid.NewGuid(), cicloId, Guid.NewGuid(), "2026-2")))).Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions))!;
        var rHuerfana = (await (await client.SendAsync(CrearSolicitudInterna(HttpMethod.Post, "/internal/v1/fondos/reservas", tokenReservar, new ReservaInternaCommandRequest(programa.Id, Guid.NewGuid(), cicloId, Guid.NewGuid(), "2026-2")))).Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions))!;
        var rFalloS4 = (await (await client.SendAsync(CrearSolicitudInterna(HttpMethod.Post, "/internal/v1/fondos/reservas", tokenReservar, new ReservaInternaCommandRequest(programa.Id, Guid.NewGuid(), cicloId, Guid.NewGuid(), "2026-2")))).Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions))!;

        // Avanzar 35 minutos y crear 1 reserva reciente (dentro de la gracia de 30 min)
        relojFijo.Avanzar(TimeSpan.FromMinutes(35));
        var rReciente = (await (await client.SendAsync(CrearSolicitudInterna(HttpMethod.Post, "/internal/v1/fondos/reservas", tokenReservar, new ReservaInternaCommandRequest(programa.Id, Guid.NewGuid(), cicloId, Guid.NewGuid(), "2026-2")))).Content.ReadFromJsonAsync<ReservaInternaResponseDto>(JsonOptions))!;

        // Avanzar 10 minutos más (las 3 primeras tienen 45 min; la reciente tiene solo 10 min)
        relojFijo.Avanzar(TimeSpan.FromMinutes(10));

        consultorStub.Configurar(rVinculable.ReservaId, new ResultadoConsultaVinculoS4(true, new EstadoVinculoReservaS4(rVinculable.ReservaId, true, "OFERTA", Guid.NewGuid(), relojFijo.AhoraUtc)));
        consultorStub.Configurar(rHuerfana.ReservaId, new ResultadoConsultaVinculoS4(true, new EstadoVinculoReservaS4(rHuerfana.ReservaId, false, "NINGUNO", null, relojFijo.AhoraUtc)));
        consultorStub.Configurar(rFalloS4.ReservaId, new ResultadoConsultaVinculoS4(false, null, "503 S4 no disponible"));

        ResultadoConciliacionReservasDto resultado;
        using (var scope = factory.Services.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            resultado = await mediator.Send(new ConciliarReservasHuerfanasCommand());
        }

        Assert.Equal(3, resultado.Evaluadas); // rReciente ignorada por plazo de gracia de 30 min
        Assert.Equal(1, resultado.Vinculadas);
        Assert.Equal(1, resultado.Liberadas);
        Assert.Equal(1, resultado.OmitidasPorFalloS4); // fail closed

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogoFondosDbContext>();
            var estados = await db.ReservasPresupuesto.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Estado);
            Assert.Equal(EstadoReservaPresupuesto.VINCULADA, estados[rVinculable.ReservaId]);
            Assert.Equal(EstadoReservaPresupuesto.LIBERADA, estados[rHuerfana.ReservaId]);
            Assert.Equal(EstadoReservaPresupuesto.RESERVADA, estados[rFalloS4.ReservaId]);
            Assert.Equal(EstadoReservaPresupuesto.RESERVADA, estados[rReciente.ReservaId]);
        }
    }

    private static HttpRequestMessage CrearSolicitudPublica(HttpMethod metodo, string ruta, string rol, object? cuerpo = null)
    {
        var req = new HttpRequestMessage(metodo, ruta);
        req.Headers.Add("X-Actor-Usuario-Id", Guid.NewGuid().ToString());
        req.Headers.Add("X-Actor-Rol", rol);
        if (cuerpo is not null)
        {
            req.Content = JsonContent.Create(cuerpo);
        }
        return req;
    }

    private static HttpRequestMessage CrearSolicitudInterna(HttpMethod metodo, string ruta, string bearerJwt, object? cuerpo = null)
    {
        var req = new HttpRequestMessage(metodo, ruta);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerJwt);
        if (cuerpo is not null)
        {
            req.Content = JsonContent.Create(cuerpo);
        }
        return req;
    }

    private static string CrearJwtServicio(string clientId, string scope) =>
        ConstruirJwt(new { azp = clientId, scope });

    private static string CrearJwtUsuarioFinal(string email, string role, string scope) =>
        ConstruirJwt(new { sub = Guid.NewGuid().ToString(), email, role, scope });

    private static string ConstruirJwt(object payload)
    {
        static string B64Url(string json) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        var header = B64Url("""{"alg":"RS256","typ":"JWT"}""");
        var body = B64Url(JsonSerializer.Serialize(payload));
        return $"{header}.{body}.firma_prueba";
    }

    private sealed class RelojFijoPruebas : IRelojNegocio
    {
        private DateTimeOffset _ahoraUtc;

        public RelojFijoPruebas(DateTimeOffset inicialUtc) => _ahoraUtc = inicialUtc;

        public DateTimeOffset AhoraUtc => _ahoraUtc;
        public DateTimeOffset AhoraCaracas => _ahoraUtc.ToOffset(TimeSpan.FromHours(-4));
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);

        public void Avanzar(TimeSpan delta) => _ahoraUtc = _ahoraUtc.Add(delta);
    }

    private sealed class ConsultorVinculoReservaS4Stub : IConsultorVinculoReservaS4
    {
        private readonly Dictionary<Guid, ResultadoConsultaVinculoS4> _respuestas = [];

        public void Configurar(Guid reservaId, ResultadoConsultaVinculoS4 resultado) =>
            _respuestas[reservaId] = resultado;

        public Task<ResultadoConsultaVinculoS4> ConsultarVinculoAsync(Guid reservaId, CancellationToken ct = default) =>
            Task.FromResult(_respuestas.TryGetValue(reservaId, out var r)
                ? r
                : new ResultadoConsultaVinculoS4(false, null, "Sin respuesta configurada"));
    }

    private sealed class PresupuestoApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly IRelojNegocio? _reloj;
        private readonly IConsultorVinculoReservaS4? _consultorS4;

        public PresupuestoApiFactory(IRelojNegocio? reloj = null, IConsultorVinculoReservaS4? consultorS4 = null)
        {
            SQLitePCL.Batteries_V2.Init();
            _dbPath = Path.Combine(Path.GetTempPath(), $"ascenso_s2_rf12_{Guid.NewGuid():N}.db");
            _connectionString = $"Data Source={_dbPath};Default Timeout=5";
            _reloj = reloj;
            _consultorS4 = consultorS4;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CatalogoFondosDbContext>>();
                services.RemoveAll<CatalogoFondosDbContext>();
                services.AddDbContext<CatalogoFondosDbContext>(options => options.UseSqlite(_connectionString));

                if (_reloj is not null)
                {
                    services.RemoveAll<IRelojNegocio>();
                    services.AddSingleton(_reloj);
                }

                if (_consultorS4 is not null)
                {
                    services.RemoveAll<IConsultorVinculoReservaS4>();
                    services.AddSingleton(_consultorS4);
                }

                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CatalogoFondosDbContext>();
                db.Database.EnsureCreated();
                db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;");
            });
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            SqliteConnection.ClearAllPools();
            foreach (var archivo in new[] { _dbPath, $"{_dbPath}-wal", $"{_dbPath}-shm" })
            {
                try
                {
                    if (File.Exists(archivo))
                    {
                        File.Delete(archivo);
                    }
                }
                catch
                {
                    // Limpieza best-effort en Windows
                }
            }
        }
    }
}
