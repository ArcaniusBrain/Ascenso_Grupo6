using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ascenso.Contracts.Eventos;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Donantes;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Programas;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.IntegrationTests;

public sealed class DonantesAportesApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task HU13_1_Registrar_Organizacion_Persona_ValidarContacto_Duplicado_Y_Roles()
    {
        await using var factory = new DonantesApiFactory();
        using var client = factory.CreateClient();

        // HU-13.1 E5: Becario, Postulante, Evaluador y Finanzas no pueden registrar donantes -> 403
        foreach (var rolBloqueado in new[] { "BECARIO", "POSTULANTE", "EVALUADOR", "FINANZAS" })
        {
            var rBloq = await client.SendAsync(CrearSolicitud(
                HttpMethod.Post,
                "/api/v1/donantes",
                rolBloqueado,
                new RegistrarDonanteRequest("Empresa Alfa", TipoDonante.ORGANIZACION, new ContactoDonanteDto("contacto@alfa.org", null))));
            Assert.Equal(HttpStatusCode.Forbidden, rBloq.StatusCode);
        }

        // HU-13.1 E3: falta información de contacto -> 422 con mensaje literal
        var rSinContacto = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/donantes",
            "COORDINADOR",
            new RegistrarDonanteRequest("Empresa Alfa", TipoDonante.ORGANIZACION, null)));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinContacto.StatusCode);
        Assert.Contains("Indica los datos de contacto del donante.", await rSinContacto.Content.ReadAsStringAsync());

        // HU-13.1 E1 & E4: Coordinador registra organización con correo y teléfono (sin cuenta en S1)
        var rOrg = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/donantes",
            "COORDINADOR",
            new RegistrarDonanteRequest(
                "Empresa Alfa",
                TipoDonante.ORGANIZACION,
                new ContactoDonanteDto("contacto@alfa.org", "+58-212-5550101"))));
        Assert.Equal(HttpStatusCode.Created, rOrg.StatusCode);
        var org = await rOrg.Content.ReadFromJsonAsync<DonanteResumenDto>(JsonOptions);
        Assert.NotNull(org);
        Assert.Equal("ORGANIZACION", org.Tipo);
        Assert.Equal(0, org.CantidadAportes);
        Assert.Equal(0m, org.TotalAportado);

        // C18: nombre duplicado insensible a mayúsculas/espacios -> 409
        var rDup = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/donantes",
            "DIRECCION",
            new RegistrarDonanteRequest(
                "  EMPRESA   ALFA ",
                TipoDonante.ORGANIZACION,
                new ContactoDonanteDto("otro@alfa.org", null))));
        Assert.Equal(HttpStatusCode.Conflict, rDup.StatusCode);

        // HU-13.1 E2: Coordinador registra persona con correo
        var rPersona = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/donantes",
            "COORDINADOR",
            new RegistrarDonanteRequest(
                "Juan Pérez",
                TipoDonante.PERSONA,
                new ContactoDonanteDto("juan.perez@correo.com", null))));
        Assert.Equal(HttpStatusCode.Created, rPersona.StatusCode);
        var persona = await rPersona.Content.ReadFromJsonAsync<DonanteResumenDto>(JsonOptions);
        Assert.NotNull(persona);
        Assert.Equal("PERSONA", persona.Tipo);
        Assert.Equal(0, persona.CantidadAportes);

        // Finanzas puede consultar lista y detalle; Becario/Postulante reciben 403
        var rListaFin = await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/donantes", "FINANZAS"));
        Assert.Equal(HttpStatusCode.OK, rListaFin.StatusCode);
        var lista = await rListaFin.Content.ReadFromJsonAsync<List<DonanteResumenDto>>(JsonOptions);
        Assert.NotNull(lista);
        Assert.Equal(2, lista.Count);

        var rDetalleFin = await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/donantes/{org.Id}", "FINANZAS"));
        Assert.Equal(HttpStatusCode.OK, rDetalleFin.StatusCode);

        var rListaBecario = await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/donantes", "BECARIO"));
        Assert.Equal(HttpStatusCode.Forbidden, rListaBecario.StatusCode);
    }

    [Fact]
    public async Task HU13_2_Aportes_AppendOnly_Validaciones_Filtro_Outbox_E_IntegracionVivaConRF11()
    {
        await using var factory = new DonantesApiFactory();
        using var client = factory.CreateClient();

        // Registrar donantes "Empresa Alfa" y "Juan Pérez" antes de crear programas
        var alfa = await (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/donantes",
            "COORDINADOR",
            new RegistrarDonanteRequest("Empresa Alfa", TipoDonante.ORGANIZACION, new ContactoDonanteDto("contacto@alfa.org", "+58-212-5550101")))))
            .Content.ReadFromJsonAsync<DonanteResumenDto>(JsonOptions);
        var juan = await (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/donantes",
            "COORDINADOR",
            new RegistrarDonanteRequest("Juan Pérez", TipoDonante.PERSONA, new ContactoDonanteDto("juan.perez@correo.com", null)))))
            .Content.ReadFromJsonAsync<DonanteResumenDto>(JsonOptions);

        // HU-13.2 E3: registrar aporte cuando no hay programas registrados -> 422 con mensaje literal
        var rSinProgramas = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/donantes/{alfa!.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteDesdeDonanteRequest(Guid.NewGuid(), 25000m, "USD", "2026-1")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinProgramas.StatusCode);
        Assert.Contains("No hay programas registrados.", await rSinProgramas.Content.ReadAsStringAsync());

        // Crear periodos: 2025-1 (cerrado), 2026-1 (cerrado) y 2026-2 (abierto)
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2025-1")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-1")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/2025-1/cerrar", "DIRECCION"))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/2026-1/cerrar", "DIRECCION"))).StatusCode);

        // Crear programas "Becas Ingeniería" y "Becas Arte"
        var progIng = await (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Ingeniería", "2026-2", 20, 8, 2000m, "USD", ModalidadPago.DIRECTO_UNIVERSIDAD, AlcancePrograma.COMPLETO))))
            .Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);
        var progArte = await (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Arte", "2026-2", 10, 8, 1000m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL))))
            .Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);

        // HU-13.2 E2: sin periodo -> mensaje literal
        var rSinPeriodo = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/donantes/{alfa.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteDesdeDonanteRequest(progIng!.Id, 25000m, "USD", "   ")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinPeriodo.StatusCode);
        Assert.Contains("Indica el periodo del aporte.", await rSinPeriodo.Content.ReadAsStringAsync());

        // HU-13.2 E7: monto 0 o negativo -> mensaje literal
        var rMontoCero = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/donantes/{alfa.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteDesdeDonanteRequest(progIng.Id, 0m, "USD", "2026-1")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rMontoCero.StatusCode);
        Assert.Contains("El monto del aporte debe ser mayor que cero.", await rMontoCero.Content.ReadAsStringAsync());

        var rMontoNeg = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/programas/{progIng.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteProgramaRequest(alfa.Id, -150m, "USD", "2026-1")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rMontoNeg.StatusCode);
        Assert.Contains("El monto del aporte debe ser mayor que cero.", await rMontoNeg.Content.ReadAsStringAsync());

        // Periodo inexistente -> 404
        var rPeriodoNoExiste = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/donantes/{alfa.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteDesdeDonanteRequest(progIng.Id, 1000m, "USD", "2030-1")));
        Assert.Equal(HttpStatusCode.NotFound, rPeriodoNoExiste.StatusCode);

        // C18: Coordinador limitado a progIng no puede registrar aporte en progArte -> 403
        var rAlcance = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/programas/{progArte!.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteProgramaRequest(alfa.Id, 5000m, "USD", "2026-2"),
            programasAsignados: [progIng.Id]));
        Assert.Equal(HttpStatusCode.Forbidden, rAlcance.StatusCode);

        // HU-13.2 E1 & E4 + C18 (periodo cerrado 2026-1 permitido):
        // Aporte 1: Empresa Alfa -> Becas Ingeniería (2026-1, 25000 USD) vía vista del donante
        var rA1 = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/donantes/{alfa.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteDesdeDonanteRequest(progIng.Id, 25000m, "USD", "2026-1")));
        Assert.Equal(HttpStatusCode.Created, rA1.StatusCode);
        var a1 = await rA1.Content.ReadFromJsonAsync<AporteDto>(JsonOptions);
        Assert.Equal("Becas Ingeniería", a1!.NombrePrograma);
        Assert.Equal(25000m, a1.Monto);

        // Aporte 2: Empresa Alfa -> Becas Ingeniería (2026-2, 30000 USD) vía ruta OpenAPI /api/v1/programas/{programaId}/aportes
        var rA2 = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/programas/{progIng.Id}/aportes",
            "COORDINADOR",
            new RegistrarAporteProgramaRequest(alfa.Id, 30000m, "USD", "2026-2")));
        Assert.Equal(HttpStatusCode.OK, rA2.StatusCode);
        var confA2 = await rA2.Content.ReadFromJsonAsync<OperacionConfirmadaDto>(JsonOptions);
        Assert.Equal("REGISTRADO", confA2!.Estado);
        Assert.Equal(2, confA2.Version);

        // Aporte 3: Empresa Alfa -> Becas Arte (2026-2, 10000 USD)
        var rA3 = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/donantes/{alfa.Id}/aportes",
            "DIRECCION",
            new RegistrarAporteDesdeDonanteRequest(progArte.Id, 10000m, "USD", "2026-2")));
        Assert.Equal(HttpStatusCode.Created, rA3.StatusCode);

        // Verificar Outbox: 3 eventos AporteRegistrado.v1 escritos atómicamente
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogoFondosDbContext>();
            var mensajes = await db.OutboxMessages
                .Where(m => m.EventType == RegistrarAporteHandler.NombreEventoIntegracion)
                .ToListAsync();
            Assert.Equal(3, mensajes.Count);
            Assert.All(mensajes, m => Assert.Equal(1, m.SchemaVersion));

            var payloads = mensajes
                .Select(m => JsonSerializer.Deserialize<AporteRegistradoV1>(m.PayloadJson, JsonOptions)!)
                .ToList();
            Assert.Equal(65000m, payloads.Sum(p => p.Monto));
            Assert.All(payloads, p => Assert.Equal("Empresa Alfa", p.NombreDonante));
        }

        // Integración viva RF-13 <-> RF-11 (ConsultorDonantesYAportesEfCore real, sin stubs):
        // Asociar Empresa Alfa y Juan Pérez a Becas Ingeniería
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/programas/{progIng.Id}/donantes", "COORDINADOR", new AsociarDonanteProgramaRequest(alfa.Id)))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/programas/{progIng.Id}/donantes", "COORDINADOR", new AsociarDonanteProgramaRequest(juan!.Id)))).StatusCode);

        var detalleIng = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/programas/{progIng.Id}", "COORDINADOR")))
            .Content.ReadFromJsonAsync<ProgramaDetalleDto>(JsonOptions);
        Assert.Equal(55000m, detalleIng!.PresupuestoTotal);
        Assert.Contains(detalleIng.Donantes, d => d.DonanteId == alfa.Id && d.TotalAportado == 55000m && d.LeyendaSinAportes is null);
        Assert.Contains(detalleIng.Donantes, d => d.DonanteId == juan.Id && d.TotalAportado == 0m && d.LeyendaSinAportes == "Sin aportes registrados");

        var detalleArte = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/programas/{progArte.Id}", "COORDINADOR")))
            .Content.ReadFromJsonAsync<ProgramaDetalleDto>(JsonOptions);
        Assert.Equal(10000m, detalleArte!.PresupuestoTotal);
        Assert.Single(detalleArte.Donantes);

        // HU-13.2 E6: Juan Pérez sin aportes -> mensaje literal
        var histJuan = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/donantes/{juan.Id}/aportes", "COORDINADOR")))
            .Content.ReadFromJsonAsync<HistorialAportesDonanteDto>(JsonOptions);
        Assert.Empty(histJuan!.Aportes);
        Assert.Equal("Este donante no tiene aportes registrados.", histJuan.MensajeSinAportes);

        // HU-13.2 E5: Historial completo y filtrado por periodo (Finanzas también puede leer)
        var histAlfaTodos = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/donantes/{alfa.Id}/aportes", "FINANZAS")))
            .Content.ReadFromJsonAsync<HistorialAportesDonanteDto>(JsonOptions);
        Assert.Equal(3, histAlfaTodos!.Aportes.Count);
        Assert.Equal(65000m, histAlfaTodos.TotalAportado);
        Assert.Null(histAlfaTodos.MensajeSinAportes);

        var histAlfa20261 = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/donantes/{alfa.Id}/aportes?codigoPeriodo=2026-1", "COORDINADOR")))
            .Content.ReadFromJsonAsync<HistorialAportesDonanteDto>(JsonOptions);
        Assert.Single(histAlfa20261!.Aportes);
        Assert.Equal(25000m, histAlfa20261.TotalAportado);
        Assert.Equal("2026-1", histAlfa20261.CodigoPeriodoFiltro);

        // HU-13.2 E9: Periodo 2025-1 sin aportes -> mensaje literal
        var histAlfa20251 = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/donantes/{alfa.Id}/aportes?codigoPeriodo=2025-1", "COORDINADOR")))
            .Content.ReadFromJsonAsync<HistorialAportesDonanteDto>(JsonOptions);
        Assert.Empty(histAlfa20251!.Aportes);
        Assert.Equal("No hay aportes de este donante en el periodo 2025-1.", histAlfa20251.MensajeSinAportes);

        // HU-13.2 E8: Postulante y Becario reciben 403 al intentar ver historial financiero
        foreach (var rolProhibido in new[] { "POSTULANTE", "BECARIO", "EVALUADOR", "ENLACE_UNIVERSIDAD" })
        {
            var rDenegado = await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/donantes/{alfa.Id}/aportes", rolProhibido));
            Assert.Equal(HttpStatusCode.Forbidden, rDenegado.StatusCode);
        }
    }

    private static HttpRequestMessage CrearSolicitud(
        HttpMethod metodo,
        string ruta,
        string rol,
        object? cuerpo = null,
        IReadOnlyCollection<Guid>? programasAsignados = null)
    {
        var req = new HttpRequestMessage(metodo, ruta);
        req.Headers.Add("X-Actor-Usuario-Id", Guid.NewGuid().ToString());
        req.Headers.Add("X-Actor-Rol", rol);
        if (programasAsignados is { Count: > 0 })
        {
            req.Headers.Add("X-Actor-Programas", string.Join(",", programasAsignados));
        }

        if (cuerpo is not null)
        {
            req.Content = JsonContent.Create(cuerpo);
        }

        return req;
    }

    private sealed class DonantesApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection;

        public DonantesApiFactory()
        {
            SQLitePCL.Batteries_V2.Init();
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CatalogoFondosDbContext>>();
                services.RemoveAll<CatalogoFondosDbContext>();
                services.AddDbContext<CatalogoFondosDbContext>(options => options.UseSqlite(_connection));

                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CatalogoFondosDbContext>();
                db.Database.EnsureCreated();
            });
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
