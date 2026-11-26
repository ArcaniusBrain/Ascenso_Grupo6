using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Programas;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Universidades;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
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

public sealed class ProgramasBecasApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task HU11_1_Crear_Validar_Roles_SinPeriodos_CorregirMayusculas_Y_ContarElementosEnPeriodo()
    {
        await using var factory = new ProgramasApiFactory();
        using var client = factory.CreateClient();

        // HU-11.1 E7: Evaluador no puede crear programas -> 403
        var rEval = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "EVALUADOR",
            new CrearProgramaRequest("Becas Ingeniería", "2026-2", 20, 8, 2000m, "USD", ModalidadPago.DIRECTO_UNIVERSIDAD, AlcancePrograma.COMPLETO)));
        Assert.Equal(HttpStatusCode.Forbidden, rEval.StatusCode);

        // HU-11.1 E3: sin periodos registrados -> mensaje literal
        var rSinPeriodos = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Ingeniería", "2026-2", 20, 8, 2000m, "USD", ModalidadPago.DIRECTO_UNIVERSIDAD, AlcancePrograma.COMPLETO)));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinPeriodos.StatusCode);
        Assert.Contains("No hay periodos académicos registrados. Pide a Dirección que los registre.", await rSinPeriodos.Content.ReadAsStringAsync());

        // Registrar periodos 2025-2 (CERRADO) y 2026-2 (ABIERTO)
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2025-2")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/2025-2/cerrar", "DIRECCION"))).StatusCode);

        // C17: periodo cerrado rechazado
        var rCerrado = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Pasadas", "2025-2", 10, 4, 1000m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL)));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rCerrado.StatusCode);

        // HU-11.1 E2, E4, E5, E6, E8, E10: validaciones literales
        Assert.Contains("Indica el cupo de becarios.", await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/programas", "COORDINADOR", new CrearProgramaRequest("Becas Arte", "2026-2", null, 8, 1000m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL)))).Content.ReadAsStringAsync());
        Assert.Contains("El cupo de becarios debe ser mayor que cero.", await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/programas", "COORDINADOR", new CrearProgramaRequest("Becas Arte", "2026-2", 0, 8, 1000m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL)))).Content.ReadAsStringAsync());
        Assert.Contains("La duración del programa debe ser mayor que cero.", await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/programas", "COORDINADOR", new CrearProgramaRequest("Becas Arte", "2026-2", 10, 0, 1000m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL)))).Content.ReadAsStringAsync());
        Assert.Contains("Indica el monto de la beca por periodo.", await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/programas", "COORDINADOR", new CrearProgramaRequest("Becas Arte", "2026-2", 10, 8, null, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL)))).Content.ReadAsStringAsync());
        Assert.Contains("El monto de la beca por periodo debe ser mayor que cero.", await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/programas", "COORDINADOR", new CrearProgramaRequest("Becas Arte", "2026-2", 10, 8, 0m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL)))).Content.ReadAsStringAsync());
        Assert.Contains("Elige si el programa es completo o parcial.", await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/programas", "COORDINADOR", new CrearProgramaRequest("Becas Arte", "2026-2", 10, 8, 1000m, "USD", ModalidadPago.MENSUAL, null)))).Content.ReadAsStringAsync());

        // HU-11.1 E1, E9, E11: crear "becas ingeniería" con monto 2000, DIRECTO_UNIVERSIDAD y COMPLETO
        var rCreado = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("becas ingeniería", "2026-2", 20, 8, 2000m, "USD", ModalidadPago.DIRECTO_UNIVERSIDAD, AlcancePrograma.COMPLETO, true)));
        Assert.Equal(HttpStatusCode.Created, rCreado.StatusCode);
        var creado = await rCreado.Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);
        Assert.NotNull(creado);
        Assert.Equal(2000m, creado.MontoPorPeriodo);
        Assert.Equal("DIRECTO_UNIVERSIDAD", creado.ModalidadPago);
        Assert.Equal("COMPLETO", creado.Alcance);
        Assert.False(creado.PermiteAcumulacion);

        // Duplicado insensible a mayúsculas -> 409
        var rDup = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "DIRECCION",
            new CrearProgramaRequest("  BECAS   INGENIERÍA ", "2026-2", 5, 4, 1000m, "USD", ModalidadPago.MENSUAL, AlcancePrograma.PARCIAL)));
        Assert.Equal(HttpStatusCode.Conflict, rDup.StatusCode);

        // Corregir mayúsculas sobre el mismo programa -> 200 OK
        var rFix = await client.SendAsync(CrearSolicitud(
            HttpMethod.Put,
            $"/api/v1/programas/{creado.Id}/configuracion-base",
            "COORDINADOR",
            new ActualizarConfiguracionProgramaRequest("Becas Ingeniería", 2000m, "USD", ModalidadPago.DIRECTO_UNIVERSIDAD, AlcancePrograma.COMPLETO, false)));
        Assert.Equal(HttpStatusCode.OK, rFix.StatusCode);
        var fijo = await rFix.Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);
        Assert.Equal("Becas Ingeniería", fijo!.Nombre);

        // Verificar que GET /api/v1/periodos/2026-2 ahora refleja 1 elemento ligado (cierre de hook RF-38)
        var detallePeriodo = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/periodos/2026-2", "COORDINADOR")))
            .Content.ReadFromJsonAsync<PeriodoDetalleDto>(JsonOptions);
        Assert.Equal(1, detallePeriodo!.CantidadElementosLigados);
        Assert.Null(detallePeriodo.MensajeSinElementos);
    }

    [Fact]
    public async Task HU11_2_Elegibilidad_Condiciones_Y_Donantes_Sin_Registros()
    {
        await using var factory = new ProgramasApiFactory();
        using var client = factory.CreateClient();

        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")));
        var programa = await (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Ingeniería", "2026-2", 20, 8, 2000m, "USD", ModalidadPago.SEMESTRAL, AlcancePrograma.COMPLETO))))
            .Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);

        // HU-11.2 E3: sin donantes registrados -> mensaje literal
        var rSinDonantes = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/programas/{programa!.Id}/donantes",
            "COORDINADOR",
            new AsociarDonanteProgramaRequest(Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinDonantes.StatusCode);
        Assert.Contains("No hay donantes registrados. Registra uno primero.", await rSinDonantes.Content.ReadAsStringAsync());

        // HU-11.2 E5: intentar restringir sin universidades aliadas registradas -> mensaje literal
        var rSinUnis = await client.SendAsync(CrearSolicitud(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/elegibilidad",
            "COORDINADOR",
            new ConfigurarElegibilidadRequest([new RestriccionUniversidadInput(Guid.NewGuid(), null)])));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinUnis.StatusCode);
        Assert.Contains("No hay universidades aliadas registradas.", await rSinUnis.Content.ReadAsStringAsync());

        // HU-11.2 E11: guardar sin restricción -> "Sin restricción de universidad ni carrera"
        var rSinRestriccion = await client.SendAsync(CrearSolicitud(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/elegibilidad",
            "COORDINADOR",
            new ConfigurarElegibilidadRequest([])));
        Assert.Equal(HttpStatusCode.OK, rSinRestriccion.StatusCode);
        var detSinRestriccion = await rSinRestriccion.Content.ReadFromJsonAsync<ProgramaDetalleDto>(JsonOptions);
        Assert.Equal("Sin restricción de universidad ni carrera", detSinRestriccion!.LeyendaElegibilidad);

        // Crear UCAB (con Ingeniería Informática) y UNIMET (sin carreras)
        var ucab = await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/universidades", "COORDINADOR", new RegistrarUniversidadRequest("UCAB", "Convenio UCAB", ViaDatosAcademicos.API, new ContactoDesignadoDto("Ana Ruiz", null, null)))))
            .Content.ReadFromJsonAsync<UniversidadResumenDto>(JsonOptions);
        await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/universidades/{ucab!.Id}/carreras", "COORDINADOR", new AgregarCarreraRequest("Ingeniería Informática")));

        var unimet = await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/universidades", "COORDINADOR", new RegistrarUniversidadRequest("UNIMET", "Convenio UNIMET", ViaDatosAcademicos.MANUAL, new ContactoDesignadoDto("Luis Gómez", null, null)))))
            .Content.ReadFromJsonAsync<UniversidadResumenDto>(JsonOptions);

        // HU-11.2 E4: carrera no registrada ("Medicina") es rechazada
        var rCarreraInvalida = await client.SendAsync(CrearSolicitud(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/elegibilidad",
            "COORDINADOR",
            new ConfigurarElegibilidadRequest([new RestriccionUniversidadInput(ucab.Id, ["Medicina"])])));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rCarreraInvalida.StatusCode);

        // HU-11.2 E4 y E12: restringir a UCAB (Ingeniería Informática) y UNIMET (solo nivel universidad -> aviso sin carreras)
        var rEleg = await client.SendAsync(CrearSolicitud(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/elegibilidad",
            "COORDINADOR",
            new ConfigurarElegibilidadRequest([
                new RestriccionUniversidadInput(ucab.Id, ["Ingeniería Informática"]),
                new RestriccionUniversidadInput(unimet!.Id, [])
            ])));
        Assert.Equal(HttpStatusCode.OK, rEleg.StatusCode);
        var detEleg = await rEleg.Content.ReadFromJsonAsync<ProgramaDetalleDto>(JsonOptions);
        Assert.Null(detEleg!.LeyendaElegibilidad);
        Assert.Contains(detEleg.Restricciones, r => r.UniversidadId == ucab.Id && r.Carreras.Contains("Ingeniería Informática"));
        Assert.Contains(detEleg.Restricciones, r => r.UniversidadId == unimet.Id && r.AvisoSinCarreras == "UNIMET no tiene carreras elegibles registradas.");

        // HU-11.2 E7, E8, E9: promedio "quince", negativo -5, horas negativas -10
        var rQuince = await client.SendAsync(CrearSolicitudRawJson(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/condiciones",
            "COORDINADOR",
            """{"promedioMinimo":"quince","exigeHorasServicio":false,"horasServicio":null,"reportesExigidos":[],"duracionMaxPausaMeses":4,"descripcionRenovacion":"Aprobar 80%"}"""));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rQuince.StatusCode);
        Assert.Contains("El promedio mínimo debe ser un número.", await rQuince.Content.ReadAsStringAsync());

        var rPromNeg = await client.SendAsync(CrearSolicitudRawJson(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/condiciones",
            "COORDINADOR",
            """{"promedioMinimo":-5,"exigeHorasServicio":false,"horasServicio":null,"reportesExigidos":[],"duracionMaxPausaMeses":4,"descripcionRenovacion":""}"""));
        Assert.Contains("El promedio mínimo no puede ser negativo.", await rPromNeg.Content.ReadAsStringAsync());

        var rHorasNeg = await client.SendAsync(CrearSolicitudRawJson(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/condiciones",
            "COORDINADOR",
            """{"promedioMinimo":15,"exigeHorasServicio":true,"horasServicio":-10,"reportesExigidos":[],"duracionMaxPausaMeses":4,"descripcionRenovacion":""}"""));
        Assert.Contains("Las horas de servicio no pueden ser negativas.", await rHorasNeg.Content.ReadAsStringAsync());

        // HU-11.2 E6: registrar condiciones completas válidas
        var rCond = await client.SendAsync(CrearSolicitudRawJson(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/condiciones",
            "COORDINADOR",
            """{"promedioMinimo":15,"exigeHorasServicio":true,"horasServicio":40,"reportesExigidos":["reporte académico por periodo"],"duracionMaxPausaMeses":4,"descripcionRenovacion":"Mantener promedio y enviar solicitud"}"""));
        Assert.Equal(HttpStatusCode.OK, rCond.StatusCode);
        var detCond = await rCond.Content.ReadFromJsonAsync<ProgramaDetalleDto>(JsonOptions);
        Assert.Equal(15m, detCond!.Condiciones!.PromedioMinimo);
        Assert.Equal(40, detCond.Condiciones.HorasServicio);
        Assert.Equal(4, detCond.Condiciones.DuracionMaxPausaMeses);

        // HU-11.2 E14: sin horas de servicio -> "Horas de servicio: no aplica"
        var rSinHoras = await client.SendAsync(CrearSolicitudRawJson(
            HttpMethod.Put,
            $"/api/v1/programas/{programa.Id}/condiciones",
            "COORDINADOR",
            """{"promedioMinimo":14,"exigeHorasServicio":false,"horasServicio":null,"reportesExigidos":["reporte académico por periodo"],"duracionMaxPausaMeses":0,"descripcionRenovacion":"Sin pausas"}"""));
        Assert.Equal(HttpStatusCode.OK, rSinHoras.StatusCode);
        var detSinHoras = await rSinHoras.Content.ReadFromJsonAsync<ProgramaDetalleDto>(JsonOptions);
        Assert.Equal("Horas de servicio: no aplica", detSinHoras!.Condiciones!.LeyendaHorasServicio);
    }

    [Fact]
    public async Task HU11_2_Donantes_Presupuesto_Suma_Aportes_Duplicado_Y_SinAportes()
    {
        var empresaAlfa = Guid.NewGuid();
        var juanPerez = Guid.NewGuid();
        var consultorStub = new ConsultorDonantesStub(empresaAlfa, juanPerez);

        await using var factory = new ProgramasApiFactory(consultorStub);
        using var client = factory.CreateClient();

        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")));
        var programa = await (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/programas",
            "COORDINADOR",
            new CrearProgramaRequest("Becas Ingeniería", "2026-2", 20, 8, 2000m, "USD", ModalidadPago.SEMESTRAL, AlcancePrograma.COMPLETO))))
            .Content.ReadFromJsonAsync<ProgramaResumenDto>(JsonOptions);

        // Asociar Empresa Alfa y Juan Pérez
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/programas/{programa!.Id}/donantes", "COORDINADOR", new AsociarDonanteProgramaRequest(empresaAlfa)))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/programas/{programa.Id}/donantes", "COORDINADOR", new AsociarDonanteProgramaRequest(juanPerez)))).StatusCode);

        // HU-11.2 E10: duplicado -> 409 con mensaje literal
        var rDup = await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/programas/{programa.Id}/donantes", "COORDINADOR", new AsociarDonanteProgramaRequest(empresaAlfa)));
        Assert.Equal(HttpStatusCode.Conflict, rDup.StatusCode);
        Assert.Contains("Este donante ya está asociado al programa.", await rDup.Content.ReadAsStringAsync());

        // HU-11.2 E13: Juan Pérez sin aportes -> "Sin aportes registrados", Empresa Alfa 30.000
        consultorStub.EstablecerAporte(programa.Id, empresaAlfa, "Empresa Alfa", 30000m, 1);
        var det1 = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/programas/{programa.Id}", "COORDINADOR")))
            .Content.ReadFromJsonAsync<ProgramaDetalleDto>(JsonOptions);
        Assert.Equal(30000m, det1!.PresupuestoTotal);
        Assert.Contains(det1.Donantes, d => d.DonanteId == juanPerez && d.LeyendaSinAportes == "Sin aportes registrados");

        // HU-11.2 E1 y E2: Juan Pérez 20.000 (total 50.000) y nuevo aporte de Empresa Alfa +10.000 (total 60.000)
        consultorStub.EstablecerAporte(programa.Id, juanPerez, "Juan Pérez", 20000m, 1);
        consultorStub.EstablecerAporte(programa.Id, empresaAlfa, "Empresa Alfa", 40000m, 2);

        var det2 = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/programas/{programa.Id}", "COORDINADOR")))
            .Content.ReadFromJsonAsync<ProgramaDetalleDto>(JsonOptions);
        Assert.Equal(60000m, det2!.PresupuestoTotal);
        Assert.Contains(det2.Donantes, d => d.DonanteId == empresaAlfa && d.TotalAportado == 40000m && d.LeyendaSinAportes is null);
        Assert.Contains(det2.Donantes, d => d.DonanteId == juanPerez && d.TotalAportado == 20000m && d.LeyendaSinAportes is null);
    }

    private static HttpRequestMessage CrearSolicitud(HttpMethod metodo, string ruta, string rol, object? body = null)
    {
        var req = new HttpRequestMessage(metodo, ruta);
        req.Headers.Add("X-Actor-Usuario-Id", Guid.NewGuid().ToString());
        req.Headers.Add("X-Actor-Rol", rol);
        if (body is not null)
        {
            req.Content = JsonContent.Create(body);
        }

        return req;
    }

    private static HttpRequestMessage CrearSolicitudRawJson(HttpMethod metodo, string ruta, string rol, string json)
    {
        var req = new HttpRequestMessage(metodo, ruta);
        req.Headers.Add("X-Actor-Usuario-Id", Guid.NewGuid().ToString());
        req.Headers.Add("X-Actor-Rol", rol);
        req.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return req;
    }

    private sealed class ConsultorDonantesStub : IConsultorDonantesYAportes
    {
        private readonly Dictionary<Guid, DonanteRegistradoInfo> _donantes;
        private readonly Dictionary<(Guid ProgramaId, Guid DonanteId), AportesDonanteEnPrograma> _aportes = [];

        public ConsultorDonantesStub(Guid empresaAlfa, Guid juanPerez)
        {
            _donantes = new Dictionary<Guid, DonanteRegistradoInfo>
            {
                [empresaAlfa] = new(empresaAlfa, "Empresa Alfa"),
                [juanPerez] = new(juanPerez, "Juan Pérez")
            };
        }

        public void EstablecerAporte(Guid programaId, Guid donanteId, string nombre, decimal total, int cantidad) =>
            _aportes[(programaId, donanteId)] = new(donanteId, nombre, total, "USD", cantidad);

        public Task<bool> ExistenDonantesRegistradosAsync(CancellationToken ct = default) =>
            Task.FromResult(_donantes.Count > 0);

        public Task<DonanteRegistradoInfo?> ObtenerDonanteAsync(Guid donanteId, CancellationToken ct = default) =>
            Task.FromResult(_donantes.TryGetValue(donanteId, out var info) ? info : null);

        public Task<IReadOnlyDictionary<Guid, AportesDonanteEnPrograma>> ObtenerAportesPorProgramaAsync(
            Guid programaId,
            CancellationToken ct = default)
        {
            IReadOnlyDictionary<Guid, AportesDonanteEnPrograma> res = _aportes
                .Where(kv => kv.Key.ProgramaId == programaId)
                .ToDictionary(kv => kv.Key.DonanteId, kv => kv.Value);
            return Task.FromResult(res);
        }
    }

    private sealed class ProgramasApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _conexion;
        private readonly IConsultorDonantesYAportes? _consultorDonantes;

        public ProgramasApiFactory(IConsultorDonantesYAportes? consultorDonantes = null)
        {
            SQLitePCL.Batteries_V2.Init();
            _conexion = new SqliteConnection("DataSource=:memory:");
            _conexion.Open();
            _consultorDonantes = consultorDonantes;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<CatalogoFondosDbContext>>();
                services.AddDbContext<CatalogoFondosDbContext>(options =>
                {
                    options.UseSqlite(_conexion);
                });

                if (_consultorDonantes is not null)
                {
                    services.RemoveAll<IConsultorDonantesYAportes>();
                    services.AddSingleton(_consultorDonantes);
                }

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CatalogoFondosDbContext>();
                db.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _conexion.Dispose();
            }
        }
    }
}
