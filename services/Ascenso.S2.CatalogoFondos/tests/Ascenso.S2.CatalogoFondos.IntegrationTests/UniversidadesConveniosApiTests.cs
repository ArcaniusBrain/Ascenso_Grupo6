using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ascenso.Contracts.Eventos;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Universidades;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Ascenso.S2.CatalogoFondos.IntegrationTests;

public sealed class UniversidadesConveniosApiTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task HU14_1_Registrar_Consultar_Aislar_Carreras_Corregir_Mayusculas_Y_Bloquear_Enlace()
    {
        await using var factory = new UniversidadesApiFactory();
        using var client = factory.CreateClient();

        // HU-14.1 E2: falta contacto designado -> 422 con mensaje literal
        var rSinContacto = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/universidades",
            "COORDINADOR",
            new RegistrarUniversidadRequest("UCAB", "Convenio 2026", ViaDatosAcademicos.API, null)));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinContacto.StatusCode);
        Assert.Contains("Indica el contacto designado de la universidad.", await rSinContacto.Content.ReadAsStringAsync());

        // HU-14.1 E8: faltan términos del convenio -> 422 con mensaje literal
        var rSinTerminos = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/universidades",
            "COORDINADOR",
            new RegistrarUniversidadRequest("UCAB", "   ", ViaDatosAcademicos.API, new ContactoDesignadoDto("Ana Ruiz", null, null))));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinTerminos.StatusCode);
        Assert.Contains("Indica los términos del convenio.", await rSinTerminos.Content.ReadAsStringAsync());

        // HU-14.1 E1 y E9: registrar ucab en minúsculas con vía API
        var rUcab = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/universidades",
            "COORDINADOR",
            new RegistrarUniversidadRequest("ucab", "Convenio marco UCAB", ViaDatosAcademicos.API, new ContactoDesignadoDto("Ana Ruiz", "ana@ucab.edu.ve", null))));
        Assert.Equal(HttpStatusCode.Created, rUcab.StatusCode);
        var ucab = await rUcab.Content.ReadFromJsonAsync<UniversidadResumenDto>(JsonOptions);
        Assert.NotNull(ucab);
        Assert.Equal("API", ucab.ViaDatosAcademicos);

        // Unicidad insensible a mayúsculas/espacios -> 409 Conflict
        var rDup = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/universidades",
            "DIRECCION",
            new RegistrarUniversidadRequest("  UCAB  ", "Otro convenio", ViaDatosAcademicos.MANUAL, new ContactoDesignadoDto("Otro", null, null))));
        Assert.Equal(HttpStatusCode.Conflict, rDup.StatusCode);

        // Corrección de mayúsculas/minúsculas sobre la misma universidad ("ucab" -> "UCAB", C16) -> 200 OK
        var rFixCase = await client.SendAsync(CrearSolicitud(
            HttpMethod.Put,
            $"/api/v1/universidades/{ucab.Id}/convenio",
            "COORDINADOR",
            new ActualizarConvenioRequest("UCAB", "Convenio marco UCAB 2026", ViaDatosAcademicos.API, new ContactoDesignadoDto("Ana Ruiz", "ana@ucab.edu.ve", null))));
        Assert.Equal(HttpStatusCode.OK, rFixCase.StatusCode);
        var ucabActualizada = await rFixCase.Content.ReadFromJsonAsync<UniversidadResumenDto>(JsonOptions);
        Assert.Equal("UCAB", ucabActualizada!.Nombre);

        // HU-14.1 E7: el enlace de universidad NO puede modificar el convenio de su universidad -> 403 Forbidden
        var rEnlace = await client.SendAsync(CrearSolicitud(
            HttpMethod.Put,
            $"/api/v1/universidades/{ucab.Id}/convenio",
            "ENLACE_UNIVERSIDAD",
            new ActualizarConvenioRequest("UCAB", "Convenio alterado por enlace", ViaDatosAcademicos.MANUAL, new ContactoDesignadoDto("Ana Ruiz", null, null))));
        Assert.Equal(HttpStatusCode.Forbidden, rEnlace.StatusCode);

        // HU-14.1 E10: registrar UNIMET con vía MANUAL
        var rUnimet = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/universidades",
            "DIRECCION",
            new RegistrarUniversidadRequest("UNIMET", "Convenio UNIMET", ViaDatosAcademicos.MANUAL, new ContactoDesignadoDto("Luis Gómez", null, null))));
        Assert.Equal(HttpStatusCode.Created, rUnimet.StatusCode);
        var unimet = await rUnimet.Content.ReadFromJsonAsync<UniversidadResumenDto>(JsonOptions);
        Assert.Equal("MANUAL", unimet!.ViaDatosAcademicos);

        // HU-14.1 E3: disponible en lista para postulantes
        var rListaPostulante = await client.SendAsync(CrearSolicitud(HttpMethod.Get, "/api/v1/universidades", "POSTULANTE"));
        Assert.Equal(HttpStatusCode.OK, rListaPostulante.StatusCode);
        var lista = await rListaPostulante.Content.ReadFromJsonAsync<List<UniversidadResumenDto>>(JsonOptions);
        Assert.NotNull(lista);
        Assert.Contains(lista, u => u.Nombre == "UNIMET");
        Assert.Contains(lista, u => u.Nombre == "UCAB");

        // HU-14.1 E5: nombre de carrera vacío -> 422 con mensaje literal
        var rCarreraVacia = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/universidades/{ucab.Id}/carreras",
            "COORDINADOR",
            new AgregarCarreraRequest("   ")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rCarreraVacia.StatusCode);
        Assert.Contains("Indica el nombre de la carrera.", await rCarreraVacia.Content.ReadAsStringAsync());

        // HU-14.1 E4 y E6: agregar carrera solo a UCAB; UNIMET sigue sin carreras con mensaje literal
        var rCarrera = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/universidades/{ucab.Id}/carreras",
            "COORDINADOR",
            new AgregarCarreraRequest("Ingeniería Informática")));
        Assert.Equal(HttpStatusCode.Created, rCarrera.StatusCode);

        var detalleUcab = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/universidades/{ucab.Id}", "COORDINADOR")))
            .Content.ReadFromJsonAsync<UniversidadDetalleDto>(JsonOptions);
        var detalleUnimet = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/universidades/{unimet.Id}", "COORDINADOR")))
            .Content.ReadFromJsonAsync<UniversidadDetalleDto>(JsonOptions);

        Assert.Single(detalleUcab!.Carreras);
        Assert.Null(detalleUcab.MensajeSinCarreras);
        Assert.Empty(detalleUnimet!.Carreras);
        Assert.Equal("Esta universidad no tiene carreras elegibles registradas.", detalleUnimet.MensajeSinCarreras);
    }

    [Fact]
    public async Task HU14_2_CostosMatricula_AppendOnly_SinPeriodos_Y_Publicacion_Outbox()
    {
        await using var factory = new UniversidadesApiFactory();
        using var client = factory.CreateClient();

        var ucab = await (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            "/api/v1/universidades",
            "COORDINADOR",
            new RegistrarUniversidadRequest("UCAB", "Convenio UCAB", ViaDatosAcademicos.API, new ContactoDesignadoDto("Ana Ruiz", null, null)))))
            .Content.ReadFromJsonAsync<UniversidadResumenDto>(JsonOptions);

        // HU-14.2 E3: sin periodos registrados -> mensaje literal
        var rSinPeriodos = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/universidades/{ucab!.Id}/costos-matricula",
            "COORDINADOR",
            new RegistrarCostoMatriculaRequest("2026-2", 1000m, "USD")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinPeriodos.StatusCode);
        Assert.Contains("No hay periodos académicos registrados.", await rSinPeriodos.Content.ReadAsStringAsync());

        // Registrar periodos 2026-1, 2026-2 y 2026-3
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-1")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-3")))).StatusCode);

        // HU-14.2 E4: costo en cero -> 422 con mensaje literal
        var rCero = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/universidades/{ucab.Id}/costos-matricula",
            "COORDINADOR",
            new RegistrarCostoMatriculaRequest("2026-2", 0m, "USD")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rCero.StatusCode);
        Assert.Contains("El costo de matrícula debe ser mayor que cero.", await rCero.Content.ReadAsStringAsync());

        // HU-14.2 E1 y E2: registrar 900 en 2026-1, 1000 en 2026-2 y corregir a 1050 en 2026-2
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/universidades/{ucab.Id}/costos-matricula", "COORDINADOR", new RegistrarCostoMatriculaRequest("2026-1", 900m, "USD")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/universidades/{ucab.Id}/costos-matricula", "COORDINADOR", new RegistrarCostoMatriculaRequest("2026-2", 1000m, "USD")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, $"/api/v1/universidades/{ucab.Id}/costos-matricula", "COORDINADOR", new RegistrarCostoMatriculaRequest("2026-2", 1050m, "USD")))).StatusCode);

        var respP2 = await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/universidades/{ucab.Id}/costos-matricula/2026-2", "COORDINADOR"));
        Assert.Equal(HttpStatusCode.OK, respP2.StatusCode);
        var costoP2 = await respP2.Content.ReadFromJsonAsync<CostoMatriculaPeriodoDto>(JsonOptions);
        Assert.NotNull(costoP2);
        Assert.Null(costoP2.Mensaje);
        Assert.Equal(1050m, costoP2.CostoVigente!.Monto);
        Assert.Equal(2, costoP2.CostoVigente.VersionRegistro);
        Assert.Equal([1000m, 1050m], costoP2.Historial.Select(h => h.Monto).ToArray());

        // HU-14.2 E5: periodo registrado sin costo cargado -> 200 con mensaje literal
        var respSinCosto = await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/universidades/{ucab.Id}/costos-matricula/2026-3", "COORDINADOR"));
        Assert.Equal(HttpStatusCode.OK, respSinCosto.StatusCode);
        var sinCosto = await respSinCosto.Content.ReadFromJsonAsync<CostoMatriculaPeriodoDto>(JsonOptions);
        Assert.Null(sinCosto!.CostoVigente);
        Assert.Empty(sinCosto.Historial);
        Assert.Equal("Sin costo registrado para este periodo", sinCosto.Mensaje);

        // Verificar Outbox: 3 eventos CostoMatriculaVigente.v1 persistidos atómicamente
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogoFondosDbContext>();
        var mensajes = await db.OutboxMessages
            .Where(m => m.EventType == RegistrarCostoMatriculaHandler.NombreEventoIntegracion)
            .ToListAsync();
        Assert.Equal(3, mensajes.Count);

        var payloads = mensajes
            .Select(m => JsonSerializer.Deserialize<CostoMatriculaVigenteV1>(m.PayloadJson, JsonOptions)!)
            .ToArray();
        Assert.Contains(payloads, p => p.CodigoPeriodo == "2026-2" && p.Monto == 1050m && p.VersionRegistro == 2);
    }

    [Fact]
    public async Task HU14_3_FechasInicioInscripciones_PorUniversidad_En_UTC_Menos_4()
    {
        // Reloj fijado en 15/09/2026 00:00:00 UTC-4 (= 04:00:00 UTC)
        var relojFijo = new RelojPruebaFijo(new DateTimeOffset(2026, 9, 15, 4, 0, 0, TimeSpan.Zero));
        await using var factory = new UniversidadesApiFactory(relojFijo);
        using var client = factory.CreateClient();

        var ucab = await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/universidades", "COORDINADOR", new RegistrarUniversidadRequest("UCAB", "Convenio UCAB", ViaDatosAcademicos.API, new ContactoDesignadoDto("Ana Ruiz", null, null)))))
            .Content.ReadFromJsonAsync<UniversidadResumenDto>(JsonOptions);
        var unimet = await (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/universidades", "DIRECCION", new RegistrarUniversidadRequest("UNIMET", "Convenio UNIMET", ViaDatosAcademicos.MANUAL, new ContactoDesignadoDto("Luis Gómez", null, null)))))
            .Content.ReadFromJsonAsync<UniversidadResumenDto>(JsonOptions);

        // HU-14.3 E4: sin periodos registrados -> mensaje literal
        var rSinPeriodos = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/universidades/{ucab!.Id}/inicios-inscripciones",
            "COORDINADOR",
            new RegistrarInicioInscripcionesUniversidadRequest("2026-2", new DateOnly(2026, 9, 15))));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinPeriodos.StatusCode);
        Assert.Contains("No hay periodos académicos registrados.", await rSinPeriodos.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2026-2")))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(HttpMethod.Post, "/api/v1/periodos", "DIRECCION", new AbrirPeriodoRequest("2027-1")))).StatusCode);

        // HU-14.3 E5: fecha vacía -> mensaje literal
        var rSinFecha = await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/universidades/{ucab.Id}/inicios-inscripciones",
            "COORDINADOR",
            new RegistrarInicioInscripcionesUniversidadRequest("2026-2", null)));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rSinFecha.StatusCode);
        Assert.Contains("Indica la fecha de inicio de inscripciones.", await rSinFecha.Content.ReadAsStringAsync());

        // HU-14.3 E1 y E2: UCAB inicia el 15/09/2026 (iniciadas = true) y UNIMET el 01/10/2026 (iniciadas = false)
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/universidades/{ucab.Id}/inicios-inscripciones",
            "COORDINADOR",
            new RegistrarInicioInscripcionesUniversidadRequest("2026-2", new DateOnly(2026, 9, 15))))).StatusCode);

        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(CrearSolicitud(
            HttpMethod.Post,
            $"/api/v1/universidades/{unimet!.Id}/inicios-inscripciones",
            "DIRECCION",
            new RegistrarInicioInscripcionesUniversidadRequest("2026-2", new DateOnly(2026, 10, 1))))).StatusCode);

        var respUcab = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/universidades/{ucab.Id}/inicios-inscripciones/2026-2", "COORDINADOR")))
            .Content.ReadFromJsonAsync<InicioInscripcionesUniversidadDto>(JsonOptions);
        var respUnimet = await (await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/universidades/{unimet.Id}/inicios-inscripciones/2026-2", "COORDINADOR")))
            .Content.ReadFromJsonAsync<InicioInscripcionesUniversidadDto>(JsonOptions);

        Assert.True(respUcab!.InscripcionesIniciadas);
        Assert.False(respUnimet!.InscripcionesIniciadas);

        // HU-14.3 E6: periodo registrado sin fecha cargada -> 200 con mensaje literal e inscripcionesIniciadas = false
        var respSinFecha = await client.SendAsync(CrearSolicitud(HttpMethod.Get, $"/api/v1/universidades/{ucab.Id}/inicios-inscripciones/2027-1", "COORDINADOR"));
        Assert.Equal(HttpStatusCode.OK, respSinFecha.StatusCode);
        var sinFecha = await respSinFecha.Content.ReadFromJsonAsync<InicioInscripcionesUniversidadDto>(JsonOptions);
        Assert.Null(sinFecha!.FechaInicio);
        Assert.False(sinFecha.InscripcionesIniciadas);
        Assert.Equal("Sin fecha de inicio de inscripciones registrada para este periodo", sinFecha.Mensaje);
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

    private sealed class UniversidadesApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _conexion;
        private readonly IRelojNegocio? _relojPersonalizado;

        public UniversidadesApiFactory(IRelojNegocio? relojPersonalizado = null)
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
