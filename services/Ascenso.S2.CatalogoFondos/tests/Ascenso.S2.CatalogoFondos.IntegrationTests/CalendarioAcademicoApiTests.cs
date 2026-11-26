using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ascenso.Contracts.Eventos;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.IntegrationTests;

public sealed class CalendarioAcademicoApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task HU38_1_Calendario_Vacio_Informa_Mensaje_Literal_Y_Ordena_Cronologicamente()
    {
        await using var factory = new CatalogoFondosApiFactory();
        using var client = factory.CreateClient();

        // HU-38.1 Escenario 3: sin periodos registrados
        using var reqVacio = CrearSolicitud(HttpMethod.Get, "/api/v1/periodos", "DIRECCION");
        var respVacio = await client.SendAsync(reqVacio);
        Assert.Equal(HttpStatusCode.OK, respVacio.StatusCode);

        var listaVacia = await respVacio.Content.ReadFromJsonAsync<ListaPeriodosDto>(JsonOptions);
        Assert.NotNull(listaVacia);
        Assert.Empty(listaVacia.Periodos);
        Assert.Equal("Aún no hay periodos académicos registrados.", listaVacia.Mensaje);

        // HU-38.1 Escenario 8: Coordinador no puede registrar periodos
        using var reqCoord = CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "COORDINADOR", new AbrirPeriodoRequest("2027-1"));
        var respCoord = await client.SendAsync(reqCoord);
        Assert.Equal(HttpStatusCode.Forbidden, respCoord.StatusCode);

        // HU-38.1 Escenario 7: vacío o espacios rechazado con mensaje literal
        using var reqEspacios = CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("   "));
        var respEspacios = await client.SendAsync(reqEspacios);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, respEspacios.StatusCode);
        var cuerpoEspacios = await respEspacios.Content.ReadAsStringAsync();
        Assert.Contains("Escribe el nombre del periodo académico.", cuerpoEspacios);

        // HU-38.1 Escenario 1 y Anexo B #1: registrar en desorden y verificar orden cronológico ascendente
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2025-2")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-1")))).StatusCode);

        // HU-38.1 Escenario 2: duplicado rechazado con mensaje literal
        var respDup = await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")));
        Assert.Equal(HttpStatusCode.Conflict, respDup.StatusCode);
        Assert.Contains("El periodo 2026-2 ya existe.", await respDup.Content.ReadAsStringAsync());

        // Verificar secuencia ordenada y accesible a personal interno (Coordinador)
        var respLista = await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/periodos", "COORDINADOR"));
        Assert.Equal(HttpStatusCode.OK, respLista.StatusCode);
        var lista = await respLista.Content.ReadFromJsonAsync<ListaPeriodosDto>(JsonOptions);
        Assert.NotNull(lista);
        Assert.Null(lista.Mensaje);
        Assert.Equal(["2025-2", "2026-1", "2026-2"], lista.Periodos.Select(p => p.Codigo).ToArray());
    }

    [Fact]
    public async Task HU38_1_Consulta_Periodo_Anterior_Sin_Elementos_E_Inexistente()
    {
        await using var factory = new CatalogoFondosApiFactory();
        using var client = factory.CreateClient();

        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2025-2")));
        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-1")));
        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")));

        var marcar = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/periodos/actual",
            "DIRECCION",
            new MarcarPeriodoRequest("2026-2", Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.OK, marcar.StatusCode);

        // HU-38.1 Escenarios 4, 5 y 6: consultar periodo anterior diferenciado del actual y sin elementos ligados
        var respAnterior = await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/periodos/2026-1", "COORDINADOR"));
        Assert.Equal(HttpStatusCode.OK, respAnterior.StatusCode);
        var detalleAnterior = await respAnterior.Content.ReadFromJsonAsync<PeriodoDetalleDto>(JsonOptions);
        Assert.NotNull(detalleAnterior);
        Assert.Equal("2026-1", detalleAnterior.Codigo);
        Assert.False(detalleAnterior.EsActual);
        Assert.True(detalleAnterior.EsPeriodoAnterior);
        Assert.Equal(0, detalleAnterior.CantidadElementosLigados);
        Assert.Equal("El periodo 2026-1 no tiene programas, ciclos ni renovaciones.", detalleAnterior.MensajeSinElementos);

        // HU-38.1 Escenario 9: consultar periodo inexistente -> 404
        var respInexistente = await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/periodos/2024-1", "COORDINADOR"));
        Assert.Equal(HttpStatusCode.NotFound, respInexistente.StatusCode);
    }

    [Fact]
    public async Task HU38_1_E10_A_E13_Y_HU38_2_E11_Fechas_Inicio_Inscripciones_Por_Universidad()
    {
        // Reloj fijado el 01/09/2026 12:00 UTC-4 (antes del 15/09/2026 y del 01/10/2026)
        var relojFijo = new RelojPruebaFijo(new DateTimeOffset(2026, 9, 1, 16, 0, 0, TimeSpan.Zero));
        await using var factory = new CatalogoFondosApiFactory(relojFijo);
        using var client = factory.CreateClient();

        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-1")));
        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")));
        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/actual", "DIRECCION", new MarcarPeriodoRequest("2026-1", Guid.NewGuid())));

        var unimetId = Guid.NewGuid();
        var ucvId = Guid.NewGuid();

        // HU-38.1 E10 y E11: dos universidades con fechas independientes en el mismo periodo
        var rUnimet = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/periodos/2026-2/inicios-inscripciones",
            "DIRECCION",
            new RegistrarInicioInscripcionesRequest(unimetId, "Universidad Metropolitana", new DateOnly(2026, 9, 15))));
        Assert.Equal(HttpStatusCode.Created, rUnimet.StatusCode);

        var rUcv = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/periodos/2026-2/inicios-inscripciones",
            "DIRECCION",
            new RegistrarInicioInscripcionesRequest(ucvId, "Universidad Central", new DateOnly(2026, 10, 1))));
        Assert.Equal(HttpStatusCode.Created, rUcv.StatusCode);

        // HU-38.1 E12: fecha vacía es rechazada
        var rVacia = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/periodos/2026-2/inicios-inscripciones",
            "DIRECCION",
            new RegistrarInicioInscripcionesRequest(Guid.NewGuid(), "Universidad Simón Bolívar", null)));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rVacia.StatusCode);

        // HU-38.1 E13: segunda fecha para la misma universidad en 2026-2 rechazada (conserva 15/09/2026)
        var rDuplicada = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/periodos/2026-2/inicios-inscripciones",
            "DIRECCION",
            new RegistrarInicioInscripcionesRequest(unimetId, "Universidad Metropolitana", new DateOnly(2026, 9, 25))));
        Assert.Equal(HttpStatusCode.Conflict, rDuplicada.StatusCode);

        // HU-38.2 E11: marcar 2026-2 como actual NO cierra ni altera las fechas de inicio de inscripciones
        var rActual = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/periodos/actual",
            "DIRECCION",
            new MarcarPeriodoRequest("2026-2", Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.OK, rActual.StatusCode);

        var respDetalle = await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/periodos/2026-2", "DIRECCION"));
        var detalle = await respDetalle.Content.ReadFromJsonAsync<PeriodoDetalleDto>(JsonOptions);
        Assert.NotNull(detalle);
        Assert.True(detalle.EsActual);
        Assert.Equal(2, detalle.IniciosInscripciones.Count);
        Assert.Contains(detalle.IniciosInscripciones, i =>
            i.UniversidadId == unimetId && i.FechaInicio == new DateOnly(2026, 9, 15) && !i.InscripcionesIniciadas);
        Assert.Contains(detalle.IniciosInscripciones, i =>
            i.UniversidadId == ucvId && i.FechaInicio == new DateOnly(2026, 10, 1) && !i.InscripcionesIniciadas);
    }

    [Fact]
    public async Task HU38_2_MarcarActual_Publica_Outbox_Atomicamente_Es_Idempotente_Y_Bloquea_No_Direccion()
    {
        await using var factory = new CatalogoFondosApiFactory();
        using var client = factory.CreateClient();

        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2025-2")));
        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-1")));
        await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")));

        // HU-38.2 E5: Coordinador no puede marcar periodo actual
        var respCoord = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/periodos/actual",
            "COORDINADOR",
            new MarcarPeriodoRequest("2026-1", Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.Forbidden, respCoord.StatusCode);

        // Anexo B HU-38.2 #3: cerrar 2025-2 e intentar marcarlo actual -> 422
        var respCerrar = await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/2025-2/cerrar", "DIRECCION"));
        Assert.Equal(HttpStatusCode.OK, respCerrar.StatusCode);
        var respMarcarCerrado = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/periodos/actual",
            "DIRECCION",
            new MarcarPeriodoRequest("2025-2", Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, respMarcarCerrado.StatusCode);

        // Marcar 2026-1 y luego 2026-2
        var op1 = Guid.NewGuid();
        var op2 = Guid.NewGuid();
        var r1 = await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/actual", "DIRECCION", new MarcarPeriodoRequest("2026-1", op1)));
        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);

        var r2 = await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/actual", "DIRECCION", new MarcarPeriodoRequest("2026-2", op2)));
        Assert.Equal(HttpStatusCode.OK, r2.StatusCode);
        var confirmacion2 = await r2.Content.ReadFromJsonAsync<OperacionConfirmadaDto>(JsonOptions);
        Assert.NotNull(confirmacion2);
        Assert.Equal("ACTUALIZADO", confirmacion2.Estado);
        Assert.Equal(op2, confirmacion2.CorrelationId);

        // Reintento con el mismo operationId -> idempotente
        var r2Reintento = await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/actual", "DIRECCION", new MarcarPeriodoRequest("2026-2", op2)));
        Assert.Equal(HttpStatusCode.OK, r2Reintento.StatusCode);
        var confirmacionReintento = await r2Reintento.Content.ReadFromJsonAsync<OperacionConfirmadaDto>(JsonOptions);
        Assert.Equal(confirmacion2, confirmacionReintento);

        // HU-38.2 E8: marcar 2026-2 de nuevo con otro operationId -> SIN_CAMBIOS y cero eventos nuevos
        var rRepetido = await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos/actual", "DIRECCION", new MarcarPeriodoRequest("2026-2", Guid.NewGuid())));
        Assert.Equal(HttpStatusCode.OK, rRepetido.StatusCode);
        var confirmacionRepetido = await rRepetido.Content.ReadFromJsonAsync<OperacionConfirmadaDto>(JsonOptions);
        Assert.NotNull(confirmacionRepetido);
        Assert.Equal("SIN_CAMBIOS", confirmacionRepetido.Estado);

        // Verificar Outbox en SQLite real: exactamente 2 eventos PeriodoMarcadoActual.v1
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogoFondosDbContext>();
        var mensajes = await db.OutboxMessages.ToListAsync();
        Assert.Equal(2, mensajes.Count);
        Assert.All(mensajes, m => Assert.Equal("PeriodoMarcadoActual.v1", m.EventType));

        var ultimoPayload = JsonSerializer.Deserialize<PeriodoMarcadoActualV1>(
            mensajes.Single(m => m.CorrelationId == op2).PayloadJson,
            JsonOptions);
        Assert.NotNull(ultimoPayload);
        Assert.Equal("2026-2", ultimoPayload.CodigoPeriodo);
        Assert.Equal("2026-1", ultimoPayload.CodigoPeriodoAnterior);
    }

    [Fact]
    public async Task HU38_2_E6_Dos_Cambios_Simultaneos_Dejan_Un_Unico_Periodo_Actual_Con_Version_Explicita()
    {
        SQLitePCL.Batteries_V2.Init();
        await using var conexion = new SqliteConnection("DataSource=:memory:");
        await conexion.OpenAsync();

        var options = new DbContextOptionsBuilder<CatalogoFondosDbContext>()
            .UseSqlite(conexion)
            .Options;

        await using (var setupDb = new CatalogoFondosDbContext(options))
        {
            await setupDb.Database.EnsureCreatedAsync();
            var calendario = CalendarioAcademico.CrearInstitucional();
            var ahora = DateTimeOffset.UtcNow;
            calendario.AbrirPeriodo("2026-1", ahora);
            calendario.AbrirPeriodo("2026-2", ahora);
            calendario.AbrirPeriodo("2027-1", ahora);
            calendario.MarcarActual(new CodigoPeriodo("2026-1"), ahora);
            setupDb.CalendariosAcademicos.Add(calendario);
            await setupDb.SaveChangesAsync();
        }

        // Dos transacciones simultáneas cargan exactamente la misma versión del calendario
        await using var dbA = new CatalogoFondosDbContext(options);
        await using var dbB = new CatalogoFondosDbContext(options);

        var calA = await dbA.CalendariosAcademicos.Include(c => c.Periodos).SingleAsync();
        var calB = await dbB.CalendariosAcademicos.Include(c => c.Periodos).SingleAsync();
        Assert.Equal(calA.Version, calB.Version);

        calA.MarcarActual(new CodigoPeriodo("2026-2"), DateTimeOffset.UtcNow);
        calB.MarcarActual(new CodigoPeriodo("2027-1"), DateTimeOffset.UtcNow);

        // El primero hace commit
        await dbA.SaveChangesAsync();

        // El segundo falla por concurrencia optimista (ADR-25 -> HTTP 409 Conflict en la API)
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => dbB.SaveChangesAsync());

        // Verificar que en BD quedó un único periodo actual sin estados intermedios
        await using var verificacionDb = new CatalogoFondosDbContext(options);
        var final = await verificacionDb.CalendariosAcademicos.Include(c => c.Periodos).SingleAsync();
        var actuales = final.Periodos.Where(p => p.EsActual).ToArray();
        Assert.Single(actuales);
        Assert.Equal("2026-2", actuales[0].Codigo.Valor);
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

    private sealed class RelojPruebaFijo : IRelojNegocio
    {
        public RelojPruebaFijo(DateTimeOffset ahoraUtc) => AhoraUtc = ahoraUtc;

        public DateTimeOffset AhoraUtc { get; }
        public DateTimeOffset AhoraCaracas => AhoraUtc.ToOffset(RelojSistemaCaracas.DesfaseCaracas);
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);
    }

    private sealed class CatalogoFondosApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _conexion;
        private readonly IRelojNegocio? _relojPersonalizado;

        public CatalogoFondosApiFactory(IRelojNegocio? relojPersonalizado = null)
        {
            SQLitePCL.Batteries_V2.Init();
            _conexion = new SqliteConnection("DataSource=:memory:");
            _conexion.Open();
            _relojPersonalizado = relojPersonalizado;
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

                if (_relojPersonalizado is not null)
                {
                    services.RemoveAll<IRelojNegocio>();
                    services.AddSingleton(_relojPersonalizado);
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
