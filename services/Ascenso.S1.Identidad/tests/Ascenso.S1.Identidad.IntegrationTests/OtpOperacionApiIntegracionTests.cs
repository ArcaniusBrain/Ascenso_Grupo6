using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ascenso.Messaging.Outbox;
using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.S1.Identidad.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Ascenso.S1.Identidad.IntegrationTests;

public sealed class OtpOperacionApiIntegracionTests
{
    private sealed class RelojControlable(DateTimeOffset inicialUtc) : IRelojNegocio
    {
        public DateTimeOffset AhoraUtc { get; set; } = inicialUtc;
        public DateTimeOffset AhoraCaracas => TimeZoneInfo.ConvertTime(AhoraUtc, TimeZoneInfo.CreateCustomTimeZone("VET", TimeSpan.FromHours(-4), "VET", "VET"));
        public DateOnly HoyCaracas => DateOnly.FromDateTime(AhoraCaracas.DateTime);
    }

    private sealed class CanalEnvioEspia : ICanalEnvioCodigo
    {
        public bool SimularFallo { get; set; }
        public List<(CanalCodigo Canal, string Destino, string Codigo)> Envios { get; } = new();

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
                throw new ExcepcionEnvioCodigo("Canal de envío temporalmente no disponible.");
            }

            Envios.Add((canal, destino, codigo));
            return Task.CompletedTask;
        }
    }

    private sealed class OtpWebAppFactory : WebApplicationFactory<Program>
    {
        public SqliteConnection Conexion { get; }
        public RelojControlable Reloj { get; } = new(new DateTimeOffset(2026, 11, 25, 16, 0, 0, TimeSpan.Zero));
        public CanalEnvioEspia CanalEnvio { get; } = new();
        public Guid PabloFinanzasId { get; } = Guid.NewGuid();
        public Guid MariaBecariaId { get; } = Guid.NewGuid();

        public OtpWebAppFactory()
        {
            Conexion = new SqliteConnection("Data Source=:memory:");
            Conexion.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<IdentidadDbContext>>();
                services.RemoveAll<IdentidadDbContext>();
                services.AddDbContext<IdentidadDbContext>((sp, options) =>
                {
                    options.UseSqlite(Conexion);
                    options.AddInterceptors(sp.GetRequiredService<AppendOnlyRegistroDecisionInterceptor>());
                });

                services.RemoveAll<IRelojNegocio>();
                services.AddSingleton<IRelojNegocio>(Reloj);

                services.RemoveAll<ICanalEnvioCodigo>();
                services.AddSingleton<ICanalEnvioCodigo>(CanalEnvio);
            });
        }

        public async Task InicializarAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IdentidadDbContext>();
            await db.Database.EnsureCreatedAsync();
            await db.Database.ExecuteSqlRawAsync(RegistroDecisionSqlAppendOnly.SqlTriggersSqlite);
            await SemillaIdentidadBase.AsegurarTiposBaseAsync(db);

            db.Usuarios.AddRange(
                Usuario.Crear(
                    new Cedula("V-19000010"),
                    "Pablo Finanzas",
                    new Contacto("pablo@ascenso.org", "+584120000010"),
                    new[] { SemillaIdentidadBase.TipoFinanzasId },
                    CanalCodigo.CORREO,
                    id: PabloFinanzasId),
                Usuario.Crear(
                    new Cedula("V-26000020"),
                    "María Becaria",
                    new Contacto("maria@ascenso.org", "+584140000020"),
                    new[] { SemillaIdentidadBase.TipoPostulanteId },
                    CanalCodigo.SMS,
                    id: MariaBecariaId));

            await db.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                Conexion.Dispose();
            }
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Flujo_Completo_Otp_Emision_FronteraPrivada_Replay_R42_Expiracion_Y_Bloqueo1Hora()
    {
        await using var factory = new OtpWebAppFactory();
        await factory.InicializarAsync();
        var client = factory.CreateClient();

        var tokenServicioS4 = CrearJwtPrueba(new { azp = "ascenso-s4-becas", scope = "fondos.reservar otp.consumir" });
        var tokenServicioSinScope = CrearJwtPrueba(new { azp = "ascenso-s3-postulacion", scope = "fondos.consultar" });
        var tokenUsuarioFinal = CrearJwtPrueba(new { sub = factory.MariaBecariaId.ToString("D"), email = "maria@ascenso.org", scope = "otp.consumir" });

        var desembolso1Id = Guid.NewGuid().ToString("D");
        var desembolso2Id = Guid.NewGuid().ToString("D");

        // 1. Emisión pública por CORREO devuelve ChallengeResponse sin exponer código
        var respSol1 = await SolicitarChallengeAsync(client, factory.PabloFinanzasId,
            new ChallengeRequest(TipoOperacionSensible.CONFIRMAR_PAGO, desembolso1Id, CanalCodigo.CORREO));
        Assert.Equal(HttpStatusCode.OK, respSol1.StatusCode);
        var rawJson = await respSol1.Content.ReadAsStringAsync();
        var ch1 = JsonSerializer.Deserialize<ChallengeResponse>(rawJson, JsonOptions)!;
        var codigo1 = factory.CanalEnvio.Envios[^1].Codigo;
        Assert.DoesNotContain(codigo1, rawJson);
        Assert.Equal(CanalCodigo.CORREO, factory.CanalEnvio.Envios[^1].Canal);

        // 2. Frontera privada (SEGURIDAD_INTERSERVICIOS_V1.1 prueba a): sin token -> 401; usuario final -> 403; servicio sin scope -> 403
        var cmdBase = new OtpInternoCommandRequest(
            factory.PabloFinanzasId, TipoOperacionSensible.CONFIRMAR_PAGO, desembolso1Id, ch1.ChallengeId, codigo1, Guid.NewGuid());

        var respSinAuth = await client.PostAsJsonAsync("/internal/v1/otp/validar-y-consumir", cmdBase);
        Assert.Equal(HttpStatusCode.Unauthorized, respSinAuth.StatusCode);

        var respPostulante = await ConsumirInternoAsync(client, tokenUsuarioFinal, cmdBase);
        Assert.Equal(HttpStatusCode.Forbidden, respPostulante.StatusCode);

        var respSinScope = await ConsumirInternoAsync(client, tokenServicioSinScope, cmdBase);
        Assert.Equal(HttpStatusCode.Forbidden, respSinScope.StatusCode);

        // 3. Vínculo incorrecto (otro recurso) rechazado con 422 sin consumir el desafío
        var cmdOtroRecurso = cmdBase with { RecursoId = desembolso2Id };
        var respOtroRecurso = await ConsumirInternoAsync(client, tokenServicioS4, cmdOtroRecurso);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, respOtroRecurso.StatusCode);

        // 4. Consumo válido por S4 con scope otp.consumir -> 200 consumido=true
        var respOk = await ConsumirInternoAsync(client, tokenServicioS4, cmdBase);
        Assert.Equal(HttpStatusCode.OK, respOk.StatusCode);
        var bodyOk = await respOk.Content.ReadFromJsonAsync<OtpInternoResponseDto>(JsonOptions);
        Assert.True(bodyOk!.Consumido);
        Assert.Equal(ch1.ChallengeId, bodyOk.ChallengeId);

        // 5. Idempotencia por corte de red (mismo operationId) -> 200 consumido=true
        var respIdem = await ConsumirInternoAsync(client, tokenServicioS4, cmdBase);
        Assert.Equal(HttpStatusCode.OK, respIdem.StatusCode);

        // 6. R42 / HU-20.1 Escenario 4: replay con nuevo operationId -> 409 Conflict
        var respReplay = await ConsumirInternoAsync(client, tokenServicioS4, cmdBase with { OperationId = Guid.NewGuid() });
        Assert.Equal(HttpStatusCode.Conflict, respReplay.StatusCode);

        // 7. Reenvío invalida el desafío previo y solicitud por SMS funciona (HU-20.3)
        var becaMariaId = Guid.NewGuid().ToString("D");
        var chMaria1 = await (await SolicitarChallengeAsync(client, factory.MariaBecariaId,
            new ChallengeRequest(TipoOperacionSensible.MODIFICAR_DATOS_PAGO, becaMariaId, CanalCodigo.SMS)))
            .Content.ReadFromJsonAsync<ChallengeResponse>(JsonOptions);
        var codigoMariaViejo = factory.CanalEnvio.Envios[^1].Codigo;
        Assert.Equal(CanalCodigo.SMS, factory.CanalEnvio.Envios[^1].Canal);

        var chMaria2 = await (await SolicitarChallengeAsync(client, factory.MariaBecariaId,
            new ChallengeRequest(TipoOperacionSensible.MODIFICAR_DATOS_PAGO, becaMariaId, CanalCodigo.SMS)))
            .Content.ReadFromJsonAsync<ChallengeResponse>(JsonOptions);
        var codigoMariaNuevo = factory.CanalEnvio.Envios[^1].Codigo;

        var respViejo = await ConsumirInternoAsync(client, tokenServicioS4,
            new OtpInternoCommandRequest(factory.MariaBecariaId, TipoOperacionSensible.MODIFICAR_DATOS_PAGO, becaMariaId, chMaria1!.ChallengeId, codigoMariaViejo, Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Conflict, respViejo.StatusCode);

        // 8. Expiración después de 15 minutos (HU-20.1 Escenario 12)
        factory.Reloj.AhoraUtc = factory.Reloj.AhoraUtc.AddMinutes(16);
        var respExp = await ConsumirInternoAsync(client, tokenServicioS4,
            new OtpInternoCommandRequest(factory.MariaBecariaId, TipoOperacionSensible.MODIFICAR_DATOS_PAGO, becaMariaId, chMaria2!.ChallengeId, codigoMariaNuevo, Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, respExp.StatusCode);

        // 9. Fallo del canal de envío -> 503 Service Unavailable
        factory.CanalEnvio.SimularFallo = true;
        var respFalloCanal = await SolicitarChallengeAsync(client, factory.PabloFinanzasId,
            new ChallengeRequest(TipoOperacionSensible.CONFIRMAR_PAGO, desembolso2Id, CanalCodigo.CORREO));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, respFalloCanal.StatusCode);
        factory.CanalEnvio.SimularFallo = false;

        // 10. Tercer intento fallido bloquea la terna durante 1 hora (HU-20.1 Escenario 13), pero permite otro recurso
        var chBloq = await (await SolicitarChallengeAsync(client, factory.PabloFinanzasId,
            new ChallengeRequest(TipoOperacionSensible.CONFIRMAR_PAGO, desembolso2Id, CanalCodigo.CORREO)))
            .Content.ReadFromJsonAsync<ChallengeResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await ConsumirInternoAsync(client, tokenServicioS4,
            new OtpInternoCommandRequest(factory.PabloFinanzasId, TipoOperacionSensible.CONFIRMAR_PAGO, desembolso2Id, chBloq!.ChallengeId, "000001", Guid.NewGuid()))).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await ConsumirInternoAsync(client, tokenServicioS4,
            new OtpInternoCommandRequest(factory.PabloFinanzasId, TipoOperacionSensible.CONFIRMAR_PAGO, desembolso2Id, chBloq.ChallengeId, "000002", Guid.NewGuid()))).StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await ConsumirInternoAsync(client, tokenServicioS4,
            new OtpInternoCommandRequest(factory.PabloFinanzasId, TipoOperacionSensible.CONFIRMAR_PAGO, desembolso2Id, chBloq.ChallengeId, "000003", Guid.NewGuid()))).StatusCode);

        // Durante 1 hora no permite solicitar nuevo OTP para desembolso2Id, pero SÍ para otro recurso
        factory.Reloj.AhoraUtc = factory.Reloj.AhoraUtc.AddMinutes(30);
        Assert.Equal(HttpStatusCode.TooManyRequests, (await SolicitarChallengeAsync(client, factory.PabloFinanzasId,
            new ChallengeRequest(TipoOperacionSensible.CONFIRMAR_PAGO, desembolso2Id, CanalCodigo.CORREO))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await SolicitarChallengeAsync(client, factory.PabloFinanzasId,
            new ChallengeRequest(TipoOperacionSensible.CONFIRMAR_PAGO, Guid.NewGuid().ToString("D"), CanalCodigo.CORREO))).StatusCode);

        // Pasada 1 hora se desbloquea desembolso2Id
        factory.Reloj.AhoraUtc = factory.Reloj.AhoraUtc.AddMinutes(31);
        Assert.Equal(HttpStatusCode.OK, (await SolicitarChallengeAsync(client, factory.PabloFinanzasId,
            new ChallengeRequest(TipoOperacionSensible.CONFIRMAR_PAGO, desembolso2Id, CanalCodigo.CORREO))).StatusCode);
    }

    [Fact]
    public async Task Concurrencia_Consumo_Otp_Solo_Un_Consumidor_Gana_Por_Version_Explicita()
    {
        await using var factory = new OtpWebAppFactory();
        await factory.InicializarAsync();

        var recursoId = Guid.NewGuid().ToString("D");
        var challengeId = Guid.NewGuid();
        var ahora = factory.Reloj.AhoraUtc;

        var options = new DbContextOptionsBuilder<IdentidadDbContext>()
            .UseSqlite(factory.Conexion)
            .Options;

        await using (var seed = new IdentidadDbContext(options))
        {
            seed.DesafiosOtpOperacion.Add(DesafioOtpOperacion.Crear(
                factory.PabloFinanzasId,
                TipoOperacionSensible.REVOCAR_BECA,
                recursoId,
                CanalCodigo.CORREO,
                "918273",
                ahora,
                id: challengeId));
            await seed.SaveChangesAsync();
        }

        // Dos contextos cargan simultáneamente el mismo desafío (Version = 0)
        await using var ctx1 = new IdentidadDbContext(options);
        await using var ctx2 = new IdentidadDbContext(options);

        var d1 = await ctx1.DesafiosOtpOperacion.SingleAsync(x => x.Id == challengeId);
        var d2 = await ctx2.DesafiosOtpOperacion.SingleAsync(x => x.Id == challengeId);

        Assert.Equal(ResultadoConsumoOtp.CONSUMIDO, d1.ValidarYConsumir(factory.PabloFinanzasId, TipoOperacionSensible.REVOCAR_BECA, recursoId, "918273", Guid.NewGuid(), ahora.AddMinutes(1)));
        await ctx1.SaveChangesAsync();

        Assert.Equal(ResultadoConsumoOtp.CONSUMIDO, d2.ValidarYConsumir(factory.PabloFinanzasId, TipoOperacionSensible.REVOCAR_BECA, recursoId, "918273", Guid.NewGuid(), ahora.AddMinutes(1)));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => ctx2.SaveChangesAsync());
    }

    private static Task<HttpResponseMessage> SolicitarChallengeAsync(HttpClient client, Guid actorId, ChallengeRequest body)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/otp/challenges")
        {
            Content = JsonContent.Create(body)
        };
        req.Headers.Add("X-Actor-Usuario-Id", actorId.ToString("D"));
        return client.SendAsync(req);
    }

    private static Task<HttpResponseMessage> ConsumirInternoAsync(HttpClient client, string bearerToken, OtpInternoCommandRequest body)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/internal/v1/otp/validar-y-consumir")
        {
            Content = JsonContent.Create(body)
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client.SendAsync(req);
    }

    private static string CrearJwtPrueba(object payload)
    {
        static string B64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var header = B64("""{"alg":"none","typ":"JWT"}""");
        var body = B64(JsonSerializer.Serialize(payload));
        return $"{header}.{body}.sig";
    }
}
