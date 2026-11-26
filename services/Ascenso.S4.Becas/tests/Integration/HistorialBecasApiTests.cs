using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Ascenso.S4.Becas.Domain.Becas;
using Ascenso.S4.Becas.Infrastructure.Persistencia;
using Ascenso.SharedKernel.ValueObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Ascenso.S4.Becas.IntegrationTests;

public sealed class HistorialBecasApiTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<BecaDbContext>>();
                    services.AddDbContext<BecaDbContext>(options => options.UseSqlite(_connection));

                    services.AddAuthentication(TestAuthHandler.SchemeName)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
                    services.AddAuthorization();
                });
            });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BecaDbContext>();
        await db.Database.EnsureCreatedAsync();

        var mariaId = Guid.NewGuid();
        var luisId = Guid.NewGuid();
        var ingenieriaId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        db.ReferenciasPersonas.AddRange(
            new ReferenciaPersona(mariaId, "V-10000001", "María Rodríguez"),
            new ReferenciaPersona(luisId, "V-10000003", "Luis Torres"));

        db.ReferenciasProgramas.Add(
            new ReferenciaPrograma(ingenieriaId, "Becas Ingeniería", Alcance.Completo));

        db.Becas.Add(new Beca(
            Guid.NewGuid(),
            mariaId,
            ingenieriaId,
            Guid.NewGuid(),
            EstadoBeca.Suspendida,
            Alcance.Completo,
            ModalidadPago.Mensual,
            [new TramoBeca(Guid.NewGuid(), new CodigoPeriodo("2026-2"), new Dinero(1200m, "USD"), OrigenTramo.Oferta)]));

        await db.SaveChangesAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task Health_RespondeOk()
    {
        var resp = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task SinAutenticacion_Responde401()
    {
        var resp = await _client.GetAsync("/api/v1/becas/historial?cedula=V-10000001");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Escenario07_CoordinadorPorCedula_Responde200SinDatosSocioeconomicos()
    {
        using var req = CrearRequest("Coordinador", "V-99999999", "/api/v1/becas/historial?cedula=V-10000001");
        var resp = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Becas Ingeniería", json);
        Assert.DoesNotContain("ingreso", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("composicion", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Escenario03_CedulaInexistente_Responde404()
    {
        using var req = CrearRequest("Coordinador", "V-99999999", "/api/v1/becas/historial?cedula=V-12345678");
        var resp = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("No se encontró a una persona con la identificación V-12345678.", json);
    }

    [Fact]
    public async Task Escenario08_BecarioConsultandoOtraPersona_Responde403()
    {
        using var req = CrearRequest("Becario", "V-10000003", "/api/v1/becas/historial?cedula=V-10000001");
        var resp = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Escenario09_BecarioConsultaAgregadaPorPrograma_Responde403()
    {
        using var req = CrearRequest("Becario", "V-10000003", "/api/v1/becas/historial?programaId=bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var resp = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Escenario12_PostulanteConsultaAgregadaPorPrograma_Responde403()
    {
        using var req = CrearRequest("Postulante", "V-10000001", "/api/v1/becas/historial?programaId=bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var resp = await _client.SendAsync(req);

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    private static HttpRequestMessage CrearRequest(string rol, string cedula, string url)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName, "test");
        req.Headers.Add("X-Test-Rol", rol);
        req.Headers.Add("X-Test-Cedula", cedula);
        return req;
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "TestScheme";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Rol", out var rol))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim> { new(ClaimTypes.Role, rol.ToString()) };
            if (Request.Headers.TryGetValue("X-Test-Cedula", out var cedula))
            {
                claims.Add(new Claim("cedula", cedula.ToString()));
            }

            var identity = new ClaimsIdentity(claims, SchemeName);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
