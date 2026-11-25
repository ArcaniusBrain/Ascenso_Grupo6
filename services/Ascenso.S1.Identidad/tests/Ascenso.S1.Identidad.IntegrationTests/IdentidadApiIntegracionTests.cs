using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ascenso.Messaging.Outbox;
using Ascenso.S1.Identidad.Application.CasosDeUso.Autenticacion;
using Ascenso.S1.Identidad.Application.CasosDeUso.TiposUsuario;
using Ascenso.S1.Identidad.Application.CasosDeUso.Usuarios;
using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Domain.Servicios;
using Ascenso.S1.Identidad.Infrastructure.Keycloak;
using Ascenso.S1.Identidad.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Ascenso.S1.Identidad.IntegrationTests;

public sealed class IdentidadApiIntegracionTests
{
    private sealed class RelojControlable(DateTimeOffset inicialUtc) : IRelojNegocio
    {
        public DateTimeOffset AhoraUtc { get; set; } = inicialUtc;
        public DateTimeOffset AhoraCaracas => TimeZoneInfo.ConvertTime(AhoraUtc, TimeZoneInfo.CreateCustomTimeZone("VET", TimeSpan.FromHours(-4), "VET", "VET"));
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);
    }

    private sealed class KeycloakEnMemoria : IProveedorIdentidadKeycloak
    {
        private readonly Dictionary<string, (string KeycloakId, string Contrasena)> _credenciales = new(StringComparer.OrdinalIgnoreCase);
        public List<string> SesionesRevocadas { get; } = new();

        public void RegistrarCredencial(string correo, string contrasena, string? keycloakId = null) =>
            _credenciales[correo.Trim().ToLowerInvariant()] = (keycloakId ?? Guid.NewGuid().ToString("D"), contrasena);

        public Task<ResultadoValidacionCredenciales> ValidarCredencialesAsync(string correo, string contrasena, CancellationToken cancellationToken = default)
        {
            if (_credenciales.TryGetValue(correo.Trim().ToLowerInvariant(), out var item) && item.Contrasena == contrasena)
            {
                return Task.FromResult(new ResultadoValidacionCredenciales(true, item.KeycloakId, "kc-access-token", "kc-refresh-token"));
            }

            return Task.FromResult(new ResultadoValidacionCredenciales(false));
        }

        public Task<string> CrearUsuarioAsync(string correo, string nombreCompleto, string? contrasenaTemporal, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString("D");
            if (!string.IsNullOrEmpty(contrasenaTemporal))
            {
                _credenciales[correo.Trim().ToLowerInvariant()] = (id, contrasenaTemporal);
            }

            return Task.FromResult(id);
        }

        public Task CerrarSesionesUsuarioAsync(string keycloakUserId, CancellationToken cancellationToken = default)
        {
            SesionesRevocadas.Add(keycloakUserId);
            return Task.CompletedTask;
        }
    }

    private sealed class CanalEnvioEspia : ICanalEnvioCodigo
    {
        public bool SimularFallo { get; set; }
        public List<(CanalCodigo Canal, string Destino, string Codigo, bool EsRecuperacion)> Envios { get; } = new();

        public Task EnviarCodigoSegundoFactorAsync(
            CanalCodigo canal,
            string destino,
            string codigo,
            int vigenciaMinutos,
            bool esRecuperacion,
            CancellationToken cancellationToken = default)
        {
            if (SimularFallo)
            {
                throw new ExcepcionEnvioCodigo("No se pudo entregar el código de verificación. Puedes solicitar el reenvío nuevamente.");
            }

            Envios.Add((canal, destino, codigo, esRecuperacion));
            return Task.CompletedTask;
        }
    }

    private sealed class IdentidadWebAppFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _conexion;
        public RelojControlable Reloj { get; } = new(new DateTimeOffset(2026, 11, 25, 14, 0, 0, TimeSpan.Zero));
        public KeycloakEnMemoria Keycloak { get; } = new();
        public CanalEnvioEspia CanalEnvio { get; } = new();
        public Guid DireccionUsuarioId { get; } = Guid.NewGuid();

        public IdentidadWebAppFactory()
        {
            _conexion = new SqliteConnection("Data Source=:memory:");
            _conexion.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<IdentidadDbContext>>();
                services.RemoveAll<IdentidadDbContext>();
                services.AddDbContext<IdentidadDbContext>((sp, options) =>
                {
                    options.UseSqlite(_conexion);
                    options.AddInterceptors(sp.GetRequiredService<AppendOnlyRegistroDecisionInterceptor>());
                });

                services.RemoveAll<IRelojNegocio>();
                services.AddSingleton<IRelojNegocio>(Reloj);

                services.RemoveAll<IProveedorIdentidadKeycloak>();
                services.AddSingleton<IProveedorIdentidadKeycloak>(Keycloak);

                services.RemoveAll<ICanalEnvioCodigo>();
                services.AddSingleton<ICanalEnvioCodigo>(CanalEnvio);
            });
        }

        public async Task InicializarBaseAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
            await db.Database.EnsureCreatedAsync();
            await db.Database.ExecuteSqlRawAsync(RegistroDecisionSqlAppendOnly.SqlTriggersSqlite);
            await SemillaIdentidadBase.AsegurarTiposBaseAsync(db);

            if (!await db.Usuarios.AnyAsync(u => u.Id == DireccionUsuarioId))
            {
                var kcId = "kc-direccion-001";
                Keycloak.RegistrarCredencial("direccion@ascenso.org", "AdminPass2026!", kcId);
                db.Usuarios.Add(Usuario.Crear(
                    new Cedula("V-10000001"),
                    "Ana Dirección",
                    new Contacto("direccion@ascenso.org", "+584120000001"),
                    new[] { SemillaIdentidadBase.TipoDireccionId },
                    CanalCodigo.CORREO,
                    keycloakUserId: kcId,
                    id: DireccionUsuarioId));
                await db.SaveChangesAsync();
            }
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

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task HU37_1_Flujo_Completo_Tipos_Usuarios_Restricciones_Y_Cierre_Sesiones()
    {
        await using var factory = new IdentidadWebAppFactory();
        await factory.InicializarBaseAsync();
        var client = factory.CreateClient();

        // Escenario 9: Tipo de usuario sin nombre -> 422 con mensaje exacto
        var reqSinNombre = ConActor(HttpMethod.Post, "/api/v1/tipos-usuario", factory.DireccionUsuarioId,
            new CrearTipoUsuarioRequest("   ", new[] { CatalogoPermisos.ConsultasResponder }));
        var respSinNombre = await client.SendAsync(reqSinNombre);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, respSinNombre.StatusCode);
        var probSinNombre = await respSinNombre.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.Equal("Escribe el nombre del tipo de usuario.", probSinNombre!.Detail);

        // Escenario 3: Crear tipo nuevo "Asistente de comunicaciones" (exento de 2FA)
        var respAsistente = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/tipos-usuario", factory.DireccionUsuarioId,
            new CrearTipoUsuarioRequest("Asistente de comunicaciones", new[] { CatalogoPermisos.ConsultasResponder })));
        Assert.Equal(HttpStatusCode.Created, respAsistente.StatusCode);
        var asistenteDto = await respAsistente.Content.ReadFromJsonAsync<TipoUsuarioDto>(JsonOptions);
        Assert.False(asistenteDto!.ExigeSegundoFactor);

        // Escenario 11: Crear tipo "Auditor interno" con acceso financiero -> exige 2FA
        var respAuditor = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/tipos-usuario", factory.DireccionUsuarioId,
            new CrearTipoUsuarioRequest("Auditor interno", new[] { CatalogoPermisos.FinanzasConsultar })));
        Assert.Equal(HttpStatusCode.Created, respAuditor.StatusCode);
        var auditorDto = await respAuditor.Content.ReadFromJsonAsync<TipoUsuarioDto>(JsonOptions);
        Assert.True(auditorDto!.ExigeSegundoFactor);

        // Escenario 7: Nombre duplicado -> 409 con mensaje exacto
        var respDup = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/tipos-usuario", factory.DireccionUsuarioId,
            new CrearTipoUsuarioRequest("Auditor interno", new[] { CatalogoPermisos.FinanzasConsultar })));
        Assert.Equal(HttpStatusCode.Conflict, respDup.StatusCode);
        var probDup = await respDup.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.Equal("El tipo de usuario Auditor interno ya existe.", probDup!.Detail);

        // Escenario 10: Usuario sin rol -> 422 con mensaje exacto
        var respSinRol = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest("V-22334455", "Carlos Sin Rol", "carlos@ascenso.org", "+584122223344", Array.Empty<Guid>())));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, respSinRol.StatusCode);
        var probSinRol = await respSinRol.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.Equal("Asigna un rol al usuario.", probSinRol!.Detail);

        // Escenario 1: Dirección crea a Laura (coordinadora) y al profesor Ramírez (evaluador externo)
        var respLaura = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest(
                "V-18111222", "Laura Coordinadora", "laura@ascenso.org", "+584141112233",
                new[] { SemillaIdentidadBase.TipoCoordinadorId }, CanalCodigo.SMS, ContrasenaTemporal: "LauraPass2026!")));
        Assert.Equal(HttpStatusCode.Created, respLaura.StatusCode);
        var lauraDto = await respLaura.Content.ReadFromJsonAsync<UsuarioDto>(JsonOptions);
        Assert.True(lauraDto!.RequiereSegundoFactor);

        var respRamirez = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest(
                "V-12999888", "Profesor Ramírez", "ramirez@uni.edu", "+584149998877",
                new[] { SemillaIdentidadBase.TipoEvaluadorId }, CanalCodigo.SMS, EsEvaluadorExterno: true, ContrasenaTemporal: "RamirezPass2026!")));
        Assert.Equal(HttpStatusCode.Created, respRamirez.StatusCode);
        var ramirezDto = await respRamirez.Content.ReadFromJsonAsync<UsuarioDto>(JsonOptions);
        Assert.True(ramirezDto!.EsEvaluadorExterno);
        Assert.Equal(CanalCodigo.CORREO, ramirezDto.CanalPreferido);

        // Escenario 2: Coordinador (Laura) intenta crear usuario -> 403
        var respNoDir = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", lauraDto.Id,
            new CrearUsuarioInternoRequest("V-25556667", "Intento", "intento@ascenso.org", "+584125556677", new[] { SemillaIdentidadBase.TipoCoordinadorId })));
        Assert.Equal(HttpStatusCode.Forbidden, respNoDir.StatusCode);

        // Escenario 8: Auto-escalamiento de Laura a Dirección -> 403
        var respEscalar = await client.SendAsync(ConActor(HttpMethod.Put, $"/api/v1/usuarios/{lauraDto.Id}/roles", lauraDto.Id,
            new CambiarRolesRequest(new[] { SemillaIdentidadBase.TipoDireccionId })));
        Assert.Equal(HttpStatusCode.Forbidden, respEscalar.StatusCode);

        // C14: Asignar "Auditor interno" a un usuario impide eliminar el tipo en uso, y actualizar permisos revoca sesiones en Keycloak
        var respUsrAuditor = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest("V-24445556", "Sofía Auditora", "sofia@ascenso.org", "+584124445566", new[] { auditorDto.Id }, ContrasenaTemporal: "SofiaPass2026!")));
        Assert.Equal(HttpStatusCode.Created, respUsrAuditor.StatusCode);

        var respDeleteEnUso = await client.SendAsync(ConActor<object?>(HttpMethod.Delete, $"/api/v1/tipos-usuario/{auditorDto.Id}", factory.DireccionUsuarioId, null));
        Assert.Equal(HttpStatusCode.Conflict, respDeleteEnUso.StatusCode);

        var respUpdatePerms = await client.SendAsync(ConActor(HttpMethod.Put, $"/api/v1/tipos-usuario/{auditorDto.Id}/permisos", factory.DireccionUsuarioId,
            new ActualizarPermisosRequest(new[] { CatalogoPermisos.FinanzasConsultar, CatalogoPermisos.RegistroDecisionesConsultarFinanciero })));
        Assert.Equal(HttpStatusCode.OK, respUpdatePerms.StatusCode);
        Assert.NotEmpty(factory.Keycloak.SesionesRevocadas);
    }

    [Fact]
    public async Task HU37_2_Todos_Los_Escenarios_2FA_Keycloak_Correo_Sms_15Min_3Fallos_Y_Recuperacion()
    {
        await using var factory = new IdentidadWebAppFactory();
        await factory.InicializarBaseAsync();
        var client = factory.CreateClient();

        // Crear usuarios de prueba: Laura (SMS), Pablo (Finanzas, correo), Ramírez (evaluador externo), Andrea (enlace universidad) y Sofía (tipo nuevo financiero)
        var respTipoAuditor = await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/tipos-usuario", factory.DireccionUsuarioId,
            new CrearTipoUsuarioRequest("Auditor interno", new[] { CatalogoPermisos.FinanzasConsultar })));
        var tipoAuditor = await respTipoAuditor.Content.ReadFromJsonAsync<TipoUsuarioDto>(JsonOptions);

        var laura = await (await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest("V-18000001", "Laura", "laura@ascenso.org", "+584140000001",
                new[] { SemillaIdentidadBase.TipoCoordinadorId }, CanalCodigo.SMS, ContrasenaTemporal: "PassLaura!"))))
            .Content.ReadFromJsonAsync<UsuarioDto>(JsonOptions);

        var pablo = await (await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest("V-18000002", "Pablo", "pablo@ascenso.org", "+584140000002",
                new[] { SemillaIdentidadBase.TipoFinanzasId }, CanalCodigo.CORREO, ContrasenaTemporal: "PassPablo!"))))
            .Content.ReadFromJsonAsync<UsuarioDto>(JsonOptions);

        await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest("V-18000003", "Profesor Ramírez", "ramirez@uni.edu", "+584140000003",
                new[] { SemillaIdentidadBase.TipoEvaluadorId }, CanalCodigo.SMS, EsEvaluadorExterno: true, ContrasenaTemporal: "PassRamirez!")));

        await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest("V-18000004", "Andrea Enlace", "andrea@ucab.edu.ve", "+584140000004",
                new[] { SemillaIdentidadBase.TipoEnlaceUniversidadId }, CanalCodigo.CORREO, UniversidadId: Guid.NewGuid(), ContrasenaTemporal: "PassAndrea!")));

        await client.SendAsync(ConActor(HttpMethod.Post, "/api/v1/usuarios", factory.DireccionUsuarioId,
            new CrearUsuarioInternoRequest("V-18000005", "Sofía Auditora", "sofia@ascenso.org", "+584140000005",
                new[] { tipoAuditor!.Id }, CanalCodigo.CORREO, ContrasenaTemporal: "PassSofia!")));

        // Escenario 6: Contraseña incorrecta NO pide ni envía 2FA
        var respBadPass = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("pablo@ascenso.org", "ClaveMala"));
        Assert.Equal(HttpStatusCode.Unauthorized, respBadPass.StatusCode);
        Assert.Empty(factory.CanalEnvio.Envios);

        // Escenarios 7 y 8: Laura (SMS) ingresa contraseña correcta -> 202 sin sesión, enviado por SMS
        var respLauraP1 = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("laura@ascenso.org", "PassLaura!"));
        Assert.Equal(HttpStatusCode.Accepted, respLauraP1.StatusCode);
        var lauraP1 = await respLauraP1.Content.ReadFromJsonAsync<ResultadoInicioSesionPaso1>(JsonOptions);
        Assert.True(lauraP1!.RequiereSegundoFactor);
        Assert.Null(lauraP1.Sesion);
        Assert.Equal(CanalCodigo.SMS, lauraP1.CanalEnviado);
        var envioLaura = factory.CanalEnvio.Envios[^1];
        Assert.Equal(CanalCodigo.SMS, envioLaura.Canal);

        // Escenario 1: Laura valida el código correcto -> abre sesión 2FA
        var respLauraP2 = await client.PostAsJsonAsync("/api/v1/auth/2fa/verificar", new Verificar2FaRequest(lauraP1.DesafioId!.Value, envioLaura.Codigo));
        Assert.Equal(HttpStatusCode.OK, respLauraP2.StatusCode);
        var sesionLaura = await respLauraP2.Content.ReadFromJsonAsync<SesionAutenticadaDto>(JsonOptions);
        Assert.True(sesionLaura!.SegundoFactorVerificado);
        Assert.False(string.IsNullOrWhiteSpace(sesionLaura.AccessToken));

        // C14: Reutilizar el código ya consumido dentro de 15 min es rechazado
        var respReuso = await client.PostAsJsonAsync("/api/v1/auth/2fa/verificar", new Verificar2FaRequest(lauraP1.DesafioId!.Value, envioLaura.Codigo));
        Assert.Equal(HttpStatusCode.Unauthorized, respReuso.StatusCode);

        // Escenario 3: Evaluador externo recibe 2FA por CORREO
        var respRamirezP1 = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("ramirez@uni.edu", "PassRamirez!"));
        Assert.Equal(HttpStatusCode.Accepted, respRamirezP1.StatusCode);
        var ramirezP1 = await respRamirezP1.Content.ReadFromJsonAsync<ResultadoInicioSesionPaso1>(JsonOptions);
        Assert.Equal(CanalCodigo.CORREO, ramirezP1!.CanalEnviado);

        // Escenario 4: Tipo nuevo financiero ("Auditor interno") exige 2FA
        var respSofiaP1 = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("sofia@ascenso.org", "PassSofia!"));
        Assert.Equal(HttpStatusCode.Accepted, respSofiaP1.StatusCode);
        Assert.True((await respSofiaP1.Content.ReadFromJsonAsync<ResultadoInicioSesionPaso1>(JsonOptions))!.RequiereSegundoFactor);

        // Escenario 11: Enlace universitario exige 2FA
        var respAndreaP1 = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("andrea@ucab.edu.ve", "PassAndrea!"));
        Assert.Equal(HttpStatusCode.Accepted, respAndreaP1.StatusCode);
        Assert.True((await respAndreaP1.Content.ReadFromJsonAsync<ResultadoInicioSesionPaso1>(JsonOptions))!.RequiereSegundoFactor);

        // Escenario 5: Fallo en el canal de envío -> 503, no abre sesión, y luego permite reenvío
        factory.CanalEnvio.SimularFallo = true;
        var respFalloEnvio = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("pablo@ascenso.org", "PassPablo!"));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, respFalloEnvio.StatusCode);
        factory.CanalEnvio.SimularFallo = false;

        // Reenvío habilitado tras fallo + C14: nuevo reenvío invalida el código previo
        var respReenvio1 = await client.PostAsJsonAsync("/api/v1/auth/2fa/reenviar", new Reenviar2FaRequest(pablo!.Id));
        Assert.Equal(HttpStatusCode.Accepted, respReenvio1.StatusCode);
        var pabloD1 = await respReenvio1.Content.ReadFromJsonAsync<ResultadoInicioSesionPaso1>(JsonOptions);
        var codigoViejo = factory.CanalEnvio.Envios[^1].Codigo;

        var respReenvio2 = await client.PostAsJsonAsync("/api/v1/auth/2fa/reenviar", new Reenviar2FaRequest(pablo.Id));
        Assert.Equal(HttpStatusCode.Accepted, respReenvio2.StatusCode);
        var pabloD2 = await respReenvio2.Content.ReadFromJsonAsync<ResultadoInicioSesionPaso1>(JsonOptions);
        var codigoNuevo = factory.CanalEnvio.Envios[^1].Codigo;

        var respCodigoViejo = await client.PostAsJsonAsync("/api/v1/auth/2fa/verificar", new Verificar2FaRequest(pabloD1!.DesafioId!.Value, codigoViejo));
        Assert.Equal(HttpStatusCode.Unauthorized, respCodigoViejo.StatusCode);

        // Escenario 9: Código vencido después de 15 minutos
        factory.Reloj.AhoraUtc = factory.Reloj.AhoraUtc.AddMinutes(16);
        var respVencido = await client.PostAsJsonAsync("/api/v1/auth/2fa/verificar", new Verificar2FaRequest(pabloD2!.DesafioId!.Value, codigoNuevo));
        Assert.Equal(HttpStatusCode.Unauthorized, respVencido.StatusCode);

        // Escenarios 2 y 10: 3 intentos fallidos -> mensaje exacto en fallo y bloqueo 1 hora en el 3ro
        var respPabloP1 = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("pablo@ascenso.org", "PassPablo!"));
        var pabloD3 = await respPabloP1.Content.ReadFromJsonAsync<ResultadoInicioSesionPaso1>(JsonOptions);

        var fallo1 = await client.PostAsJsonAsync("/api/v1/auth/2fa/verificar", new Verificar2FaRequest(pabloD3!.DesafioId!.Value, "000001"));
        Assert.Equal(HttpStatusCode.Unauthorized, fallo1.StatusCode);
        var probFallo1 = await fallo1.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        Assert.Equal("El segundo factor no es válido.", probFallo1!.Detail);

        var fallo2 = await client.PostAsJsonAsync("/api/v1/auth/2fa/verificar", new Verificar2FaRequest(pabloD3.DesafioId!.Value, "000002"));
        Assert.Equal(HttpStatusCode.Unauthorized, fallo2.StatusCode);

        var fallo3 = await client.PostAsJsonAsync("/api/v1/auth/2fa/verificar", new Verificar2FaRequest(pabloD3.DesafioId!.Value, "000003"));
        Assert.Equal(HttpStatusCode.TooManyRequests, fallo3.StatusCode);

        // Durante la hora de bloqueo no permite nuevo login 2FA
        factory.Reloj.AhoraUtc = factory.Reloj.AhoraUtc.AddMinutes(30);
        var intentoBloqueado = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("pablo@ascenso.org", "PassPablo!"));
        Assert.Equal(HttpStatusCode.TooManyRequests, intentoBloqueado.StatusCode);

        // Pasada 1 hora vuelve a permitir login 2FA
        factory.Reloj.AhoraUtc = factory.Reloj.AhoraUtc.AddMinutes(31);
        var intentoDesbloqueado = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest("pablo@ascenso.org", "PassPablo!"));
        Assert.Equal(HttpStatusCode.Accepted, intentoDesbloqueado.StatusCode);

        // Escenario 12: Recuperación por correo cuando Laura pierde el teléfono SMS
        var respRec = await client.PostAsJsonAsync("/api/v1/auth/2fa/recuperar", new Recuperar2FaRequest("laura@ascenso.org"));
        Assert.Equal(HttpStatusCode.Accepted, respRec.StatusCode);
        var recDto = await respRec.Content.ReadFromJsonAsync<ResultadoInicioSesionPaso1>(JsonOptions);
        Assert.Equal(CanalCodigo.CORREO, recDto!.CanalEnviado);
        var envioRec = factory.CanalEnvio.Envios[^1];
        Assert.True(envioRec.EsRecuperacion);
        Assert.Equal(CanalCodigo.CORREO, envioRec.Canal);
        Assert.Equal(laura!.Correo, envioRec.Destino);
    }

    [Fact]
    public async Task ProveedorIdentidadKeycloakHttp_Envia_Grant_Password_Y_Maneja_401_Y_200()
    {
        var handler = new StubHttpMessageHandler(req =>
        {
            var body = req.Content!.ReadAsStringAsync().Result;
            if (body.Contains("password=Correcta"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"access_token":"jwt-kc","refresh_token":"ref-kc"}""", Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        });

        var http = new HttpClient(handler);
        var opts = Options.Create(new KeycloakOptions
        {
            BaseUrl = "http://keycloak-interno:8080",
            Realm = "ascenso",
            ClientId = "ascenso-s1-identidad",
            ClientSecret = "secret"
        });

        var proveedor = new ProveedorIdentidadKeycloakHttp(http, opts);

        var ok = await proveedor.ValidarCredencialesAsync("laura@ascenso.org", "Correcta");
        Assert.True(ok.Exitoso);
        Assert.Equal("jwt-kc", ok.AccessTokenPrimario);

        var fail = await proveedor.ValidarCredencialesAsync("laura@ascenso.org", "Mala");
        Assert.False(fail.Exitoso);
    }

    private static HttpRequestMessage ConActor<T>(HttpMethod method, string url, Guid actorId, T? body)
    {
        var req = new HttpRequestMessage(method, url);
        req.Headers.Add("X-Actor-Usuario-Id", actorId.ToString("D"));
        if (body is not null)
        {
            req.Content = JsonContent.Create(body);
        }

        return req;
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }
}
