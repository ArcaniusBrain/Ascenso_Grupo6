using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.Consentimientos;
using Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.ReportesImpacto;
using Ascenso.S6.ComunicacionesReporteria.Application.Puertos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Ascenso.S6.ComunicacionesReporteria.IntegrationTests;

public sealed class ReportesImpactoApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task HU33_2_Consentimiento_PropioBecario_Irrevocable_Idempotente_Y_BloqueoOtrosRoles()
    {
        var reloj = new RelojFijoS6(new DateTimeOffset(2026, 10, 1, 15, 0, 0, TimeSpan.Zero));
        await using var factory = new ComunicacionesApiFactory(reloj: reloj);
        using var client = factory.CreateClient();

        var carlosId = Guid.NewGuid();
        var anaId = Guid.NewGuid();
        var donanteId = Guid.NewGuid();

        // HU-33.2 E3: Coordinador, Dirección y otro Becario NO pueden aceptar por Ana -> 403
        var rCoord = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/consentimientos-donantes", Guid.NewGuid(), "COORDINADOR",
            new RegistrarConsentimientoRequest(donanteId, true, anaId)));
        Assert.Equal(HttpStatusCode.Forbidden, rCoord.StatusCode);

        var rOtroBecario = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/consentimientos-donantes", carlosId, "BECARIO",
            new RegistrarConsentimientoRequest(donanteId, true, anaId)));
        Assert.Equal(HttpStatusCode.Forbidden, rOtroBecario.StatusCode);

        // FA-1: si no marca aceptación, no registra nada
        var rNoMarca = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/consentimientos-donantes", anaId, "BECARIO",
            new RegistrarConsentimientoRequest(donanteId, false)));
        Assert.Equal(HttpStatusCode.OK, rNoMarca.StatusCode);
        var noMarcaDto = await rNoMarca.Content.ReadFromJsonAsync<ConsentimientoRegistroResultadoDto>(JsonOptions);
        Assert.False(noMarcaDto!.Registrado);
        Assert.Null(noMarcaDto.Consentimiento);

        // HU-33.2 E1: Carlos acepta el 01-oct-2026
        var rAcepta1 = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/consentimientos-donantes", carlosId, "BECARIO",
            new RegistrarConsentimientoRequest(donanteId, true)));
        Assert.Equal(HttpStatusCode.OK, rAcepta1.StatusCode);
        var c1 = await rAcepta1.Content.ReadFromJsonAsync<ConsentimientoRegistroResultadoDto>(JsonOptions);
        Assert.True(c1!.Registrado);
        Assert.False(c1.YaExistia);
        Assert.True(c1.Consentimiento!.Irrevocable);
        var fechaOriginal = c1.Consentimiento.AceptadoEnUtc;

        // HU-33.2 E4: Carlos confirma de nuevo 15 días después -> conserva único registro y fecha original
        reloj.Avanzar(TimeSpan.FromDays(15));
        var rAcepta2 = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/consentimientos-donantes", carlosId, "BECARIO",
            new RegistrarConsentimientoRequest(donanteId, true)));
        Assert.Equal(HttpStatusCode.OK, rAcepta2.StatusCode);
        var c2 = await rAcepta2.Content.ReadFromJsonAsync<ConsentimientoRegistroResultadoDto>(JsonOptions);
        Assert.True(c2!.YaExistia);
        Assert.Equal(c1.Consentimiento.Id, c2.Consentimiento!.Id);
        Assert.Equal(fechaOriginal, c2.Consentimiento.AceptadoEnUtc);

        var mios = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/consentimientos-donantes/mios", carlosId, "BECARIO")))
            .Content.ReadFromJsonAsync<List<ConsentimientoDonanteDto>>(JsonOptions);
        Assert.Single(mios!);
    }

    [Fact]
    public async Task HU33_1_ReporteImpacto_Asincrono_MultiPrograma_Anonimizacion_DesembolsosConfirmados_Y_Validaciones()
    {
        await using var factory = new ComunicacionesApiFactory();
        using var client = factory.CreateClient();

        var donanteHorizonteId = Guid.NewGuid();
        var donanteSinAportesId = Guid.NewGuid();
        var otroDonanteId = Guid.NewGuid();
        var progIngId = Guid.NewGuid();
        var progSaludId = Guid.NewGuid();
        var carlosId = Guid.NewGuid();
        var anaId = Guid.NewGuid();

        // Sembrar proyecciones CQRS locales en s6_comunicaciones
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComunicacionesDbContext>();
            db.ProyeccionPeriodos.Add(new PeriodoCalendarioProyeccion { CodigoPeriodo = "2026-1" });
            db.ProyeccionAportes.AddRange(
                new AporteImpactoProyeccion { Id = Guid.NewGuid(), DonanteId = donanteHorizonteId, NombreDonante = "Fundación Horizonte", ProgramaId = progIngId, NombrePrograma = "Becas Ingeniería", CodigoPeriodo = "2026-1", MontoAportado = 10000m, Moneda = "USD" },
                new AporteImpactoProyeccion { Id = Guid.NewGuid(), DonanteId = donanteHorizonteId, NombreDonante = "Fundación Horizonte", ProgramaId = progSaludId, NombrePrograma = "Becas Salud", CodigoPeriodo = "2026-1", MontoAportado = 5000m, Moneda = "USD" },
                new AporteImpactoProyeccion { Id = Guid.NewGuid(), DonanteId = otroDonanteId, NombreDonante = "Patrocinador Ajeno", ProgramaId = progIngId, NombrePrograma = "Becas Ingeniería", CodigoPeriodo = "2026-1", MontoAportado = 8000m, Moneda = "USD" });

            db.ProyeccionBecarios.AddRange(
                new BecarioImpactoProyeccion { Id = Guid.NewGuid(), BecarioId = carlosId, ProgramaId = progIngId, NombreCompleto = "Carlos Mendoza", Cedula = "V-20111222", Universidad = "UCAB", Carrera = "Ingeniería Informática", IngresoFamiliarSensible = 450.73m, ComposicionFamiliarSensible = "3 personas" },
                new BecarioImpactoProyeccion { Id = Guid.NewGuid(), BecarioId = anaId, ProgramaId = progIngId, NombreCompleto = "Ana Gómez", Cedula = "V-20999888", Universidad = "UCAB", Carrera = "Ingeniería Civil", IngresoFamiliarSensible = 220.49m, ComposicionFamiliarSensible = "5 personas" });

            db.ProyeccionDesembolsos.AddRange(
                new DesembolsoImpactoProyeccion { Id = Guid.NewGuid(), ProgramaId = progIngId, CodigoPeriodo = "2026-1", Monto = 4000m, Moneda = "USD", Estado = EstadoDesembolsoProyectado.CONFIRMADO },
                new DesembolsoImpactoProyeccion { Id = Guid.NewGuid(), ProgramaId = progIngId, CodigoPeriodo = "2026-1", Monto = 500m, Moneda = "USD", Estado = EstadoDesembolsoProyectado.FALLIDO });

            db.ProyeccionReportesAcademicos.AddRange(
                new ReporteAcademicoImpactoProyeccion { Id = Guid.NewGuid(), BecarioId = carlosId, ProgramaId = progIngId, CodigoPeriodo = "2026-1", Promedio = 18.5m, MateriasAprobadas = 5, MateriasReprobadas = 0, Origen = OrigenReporteAcademicoProyectado.API },
                new ReporteAcademicoImpactoProyeccion { Id = Guid.NewGuid(), BecarioId = anaId, ProgramaId = progIngId, CodigoPeriodo = "2026-1", Promedio = 16.5m, MateriasAprobadas = 4, MateriasReprobadas = 1, Origen = OrigenReporteAcademicoProyectado.MANUAL });

            await db.SaveChangesAsync();
        }

        // Carlos registra su consentimiento para Fundación Horizonte (HU-33.2)
        Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/consentimientos-donantes", carlosId, "BECARIO",
            new RegistrarConsentimientoRequest(donanteHorizonteId, true)))).StatusCode);

        // HU-33.1 E7: Evaluador, Becario, Postulante y Finanzas NO generan reporte -> 403
        foreach (var rolBloq in new[] { "EVALUADOR", "BECARIO", "POSTULANTE", "FINANZAS", "ENLACE_UNIVERSIDAD" })
        {
            var r403 = await client.SendAsync(CrearSolicitud(
                HttpMethod.Post, "/api/v1/reportes/impacto", Guid.NewGuid(), rolBloq,
                new SolicitarReporteImpactoRequest(donanteHorizonteId, "2026-1")));
            Assert.Equal(HttpStatusCode.Forbidden, r403.StatusCode);
        }

        // HU-33.1 E8: periodo inexistente -> 404 con mensaje literal
        var rPerInex = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/reportes/impacto", Guid.NewGuid(), "COORDINADOR",
            new SolicitarReporteImpactoRequest(donanteHorizonteId, "2027-1")));
        Assert.Equal(HttpStatusCode.NotFound, rPerInex.StatusCode);
        Assert.Contains("El periodo 2027-1 no existe en el calendario académico.", await rPerInex.Content.ReadAsStringAsync());

        // HU-33.1 E2: donante sin aportes en 2026-1 -> 422 con mensaje literal
        var rSinAportes = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/reportes/impacto", Guid.NewGuid(), "COORDINADOR",
            new SolicitarReporteImpactoRequest(donanteSinAportesId, "2026-1")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinAportes.StatusCode);
        Assert.Contains("Este donante no tiene aportes registrados en el periodo 2026-1.", await rSinAportes.Content.ReadAsStringAsync());

        // HU-33.1 E1, E3, E6, E9, E10, E11, E12: Coordinador solicita reporte (ambos programas en su alcance) -> 202 Accepted
        var rSol = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/reportes/impacto", Guid.NewGuid(), "COORDINADOR",
            new SolicitarReporteImpactoRequest(donanteHorizonteId, "2026-1"),
            programasAsignados: [progIngId, progSaludId]));
        Assert.Equal(HttpStatusCode.Accepted, rSol.StatusCode);
        var solicitud = await rSol.Content.ReadFromJsonAsync<SolicitudReporteImpactoDto>(JsonOptions);
        Assert.Equal("PREPARANDO", solicitud!.Estado);
        Assert.Equal("El reporte se está preparando.", solicitud.MensajeEstado);

        // Esperar finalización del ProcesadorReportesImpactoWorker en segundo plano
        SolicitudReporteImpactoDto? listo = null;
        string? jsonCrudo = null;
        for (var intento = 0; intento < 50; intento++)
        {
            var rCons = await client.SendAsync(CrearSolicitud(
                HttpMethod.Get, $"/api/v1/reportes/impacto/{solicitud.ReporteId}", Guid.NewGuid(), "FINANZAS"));
            Assert.Equal(HttpStatusCode.OK, rCons.StatusCode);
            jsonCrudo = await rCons.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<SolicitudReporteImpactoDto>(jsonCrudo, JsonOptions);
            if (dto?.Estado == "LISTO")
            {
                listo = dto;
                break;
            }
            await Task.Delay(50);
        }

        Assert.NotNull(listo);
        Assert.NotNull(listo.Contenido);
        Assert.Equal(2, listo.Contenido.Programas.Count);
        Assert.Equal(15000m, listo.Contenido.TotalAportadoDonante);
        Assert.Equal(4000m, listo.Contenido.TotalDesembolsadoConfirmado);

        var ing = listo.Contenido.Programas.Single(p => p.ProgramaId == progIngId);
        Assert.Equal(2, ing.NumeroBecarios);
        Assert.Equal(4000m, ing.EjecucionPresupuestaria.DesembolsadoConfirmado);
        Assert.Equal(17.5m, ing.ResultadosAcademicos.PromedioGeneral);
        Assert.Single(ing.BecariosPresentados);
        Assert.Equal("Carlos Mendoza", ing.BecariosPresentados[0].NombreCompleto);
        Assert.Equal("V-20111222", ing.BecariosPresentados[0].Cedula);

        var salud = listo.Contenido.Programas.Single(p => p.ProgramaId == progSaludId);
        Assert.Equal("Aún no hay datos académicos para este periodo.", salud.ResultadosAcademicos.MensajeSinDatos);

        // Verificación estricta anti-fuga sobre el JSON real entregado por HTTP
        Assert.DoesNotContain("Ana Gómez", jsonCrudo!);
        Assert.DoesNotContain("V-20999888", jsonCrudo!);
        Assert.DoesNotContain("450.73", jsonCrudo!);
        Assert.DoesNotContain("220.49", jsonCrudo!);
        Assert.DoesNotContain("ingreso", jsonCrudo!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("composicion", jsonCrudo!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("3 personas", jsonCrudo!);
        Assert.DoesNotContain("5 personas", jsonCrudo!);
        Assert.DoesNotContain("Patrocinador Ajeno", jsonCrudo!);

        // GET /api/v1/reportes/impacto devuelve el reporte conservado para re-entrega
        var rLista = await client.SendAsync(CrearSolicitud(
            HttpMethod.Get, $"/api/v1/reportes/impacto?donanteId={donanteHorizonteId}&codigoPeriodo=2026-1", Guid.NewGuid(), "DIRECCION"));
        Assert.Equal(HttpStatusCode.OK, rLista.StatusCode);
        var lista = await rLista.Content.ReadFromJsonAsync<List<SolicitudReporteImpactoDto>>(JsonOptions);
        Assert.Single(lista!);
    }

    [Fact]
    public async Task HU33_1_E12_Reporte_En_Preparacion_No_Bloquea_Operacion_Diaria()
    {
        var colaManual = new ColaPausadaParaPrueba();
        await using var factory = new ComunicacionesApiFactory(colaOverride: colaManual);
        using var client = factory.CreateClient();

        var donanteId = Guid.NewGuid();
        var progId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComunicacionesDbContext>();
            db.ProyeccionPeriodos.Add(new PeriodoCalendarioProyeccion { CodigoPeriodo = "2026-1" });
            db.ProyeccionAportes.Add(new AporteImpactoProyeccion { Id = Guid.NewGuid(), DonanteId = donanteId, NombreDonante = "Fundación Horizonte", ProgramaId = progId, NombrePrograma = "Becas Ingeniería", CodigoPeriodo = "2026-1", MontoAportado = 3000m, Moneda = "USD" });
            await db.SaveChangesAsync();
        }

        var rSol = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post, "/api/v1/reportes/impacto", Guid.NewGuid(), "DIRECCION",
            new SolicitarReporteImpactoRequest(donanteId, "2026-1")));
        Assert.Equal(HttpStatusCode.Accepted, rSol.StatusCode);
        var sol = await rSol.Content.ReadFromJsonAsync<SolicitudReporteImpactoDto>(JsonOptions);

        // Mientras el reporte sigue PREPARANDO, otras peticiones siguen respondiendo de inmediato
        var rHealth = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, rHealth.StatusCode);

        var rEstado = await client.SendAsync(CrearSolicitud(
            HttpMethod.Get, $"/api/v1/reportes/impacto/{sol!.ReporteId}", Guid.NewGuid(), "COORDINADOR"));
        Assert.Equal(HttpStatusCode.OK, rEstado.StatusCode);
        var enCurso = await rEstado.Content.ReadFromJsonAsync<SolicitudReporteImpactoDto>(JsonOptions);
        Assert.Equal("PREPARANDO", enCurso!.Estado);
        Assert.Equal("El reporte se está preparando.", enCurso.MensajeEstado);
        Assert.Null(enCurso.Contenido);
        Assert.Contains(sol.ReporteId, colaManual.Encolados);
    }

    private static HttpRequestMessage CrearSolicitud(
        HttpMethod metodo,
        string ruta,
        Guid usuarioId,
        string rol,
        object? cuerpo = null,
        IReadOnlyCollection<Guid>? programasAsignados = null)
    {
        var req = new HttpRequestMessage(metodo, ruta);
        req.Headers.Add("X-Actor-Usuario-Id", usuarioId.ToString());
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

    private sealed class RelojFijoS6 : IRelojNegocio
    {
        private DateTimeOffset _ahoraUtc;

        public RelojFijoS6(DateTimeOffset inicialUtc) => _ahoraUtc = inicialUtc;
        public DateTimeOffset AhoraUtc => _ahoraUtc;
        public DateTimeOffset AhoraCaracas => _ahoraUtc.ToOffset(TimeSpan.FromHours(-4));
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);
        public void Avanzar(TimeSpan delta) => _ahoraUtc = _ahoraUtc.Add(delta);
    }

    private sealed class ColaPausadaParaPrueba : IColaProcesamientoReportesImpacto
    {
        public List<Guid> Encolados { get; } = [];

        public ValueTask EncolarAsync(Guid reporteId, CancellationToken ct = default)
        {
            Encolados.Add(reporteId);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ComunicacionesApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath;
        private readonly string _connectionString;
        private readonly IRelojNegocio? _reloj;
        private readonly IColaProcesamientoReportesImpacto? _colaOverride;

        public ComunicacionesApiFactory(
            IRelojNegocio? reloj = null,
            IColaProcesamientoReportesImpacto? colaOverride = null)
        {
            SQLitePCL.Batteries_V2.Init();
            _dbPath = Path.Combine(Path.GetTempPath(), $"ascenso_s6_rf33_{Guid.NewGuid():N}.db");
            _connectionString = $"Data Source={_dbPath};Default Timeout=5";
            _reloj = reloj;
            _colaOverride = colaOverride;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ComunicacionesDbContext>>();
                services.RemoveAll<ComunicacionesDbContext>();
                services.AddDbContext<ComunicacionesDbContext>(options => options.UseSqlite(_connectionString));

                if (_reloj is not null)
                {
                    services.RemoveAll<IRelojNegocio>();
                    services.AddSingleton(_reloj);
                }

                if (_colaOverride is not null)
                {
                    services.RemoveAll<IColaProcesamientoReportesImpacto>();
                    services.AddSingleton(_colaOverride);
                }

                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ComunicacionesDbContext>();
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
                }
            }
        }
    }
}
