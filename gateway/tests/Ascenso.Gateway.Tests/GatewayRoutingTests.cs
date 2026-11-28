using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ascenso.Gateway.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Yarp.ReverseProxy.Forwarder;

namespace Ascenso.Gateway.Tests;

public sealed class GatewayRoutingTests
{
    [Fact]
    public async Task Health_Retorna200YPropagaCorrelationId()
    {
        using var factory = new GatewayTestFactory();
        using var client = factory.CreateClient();

        // Sin encabezado: el Gateway genera uno nuevo y lo devuelve en la respuesta.
        var respuestaGenerada = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, respuestaGenerada.StatusCode);
        Assert.True(respuestaGenerada.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var valoresGenerados));
        Assert.False(string.IsNullOrWhiteSpace(valoresGenerados.Single()));

        var cuerpo = await respuestaGenerada.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Healthy", cuerpo.GetProperty("status").GetString());
        Assert.Equal("Ascenso.Gateway", cuerpo.GetProperty("service").GetString());

        // Con encabezado explícito: lo conserva tal cual.
        const string correlationId = "corr-gateway-test-001";
        using var peticion = new HttpRequestMessage(HttpMethod.Get, "/health");
        peticion.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        var respuestaEco = await client.SendAsync(peticion);
        Assert.Equal(HttpStatusCode.OK, respuestaEco.StatusCode);
        Assert.Equal(correlationId, respuestaEco.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single());
    }

    [Theory]
    [InlineData("/internal/v1/fondos/reservas")]
    [InlineData("/internal/v1/otp/validar-y-consumir")]
    [InlineData("/INTERNAL/v1/reservas/123/vinculo")]
    [InlineData("/api/v1/programas/%2e%2e/internal/v1/fondos/reservas")]
    [InlineData("/api/v1/programas/..%5Cinternal/v1/fondos/reservas")]
    public async Task FronteraPrivada_BloqueaRutasInternasYTraversalCon403ProblemJson(string ruta)
    {
        using var factory = new GatewayTestFactory();
        using var client = factory.CreateClient();

        var respuesta = await client.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);

        var problema = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(403, problema.GetProperty("status").GetInt32());
        Assert.Equal(FronteraPrivadaMiddleware.CodigoBloqueo, problema.GetProperty("code").GetString());
        Assert.Empty(factory.Handler.SolicitudesCapturadas);
    }

    [Theory]
    [InlineData("/api/v1/tipos-usuario", "http://s1-identidad:5101/api/v1/tipos-usuario")]
    [InlineData("/api/v1/auth/login", "http://s1-identidad:5101/api/v1/auth/login")]
    [InlineData("/api/v1/programas", "http://s2-catalogo-fondos:5102/api/v1/programas")]
    [InlineData("/api/v1/programas/abc/presupuesto?soloActivos=true", "http://s2-catalogo-fondos:5102/api/v1/programas/abc/presupuesto?soloActivos=true")]
    [InlineData("/api/v1/convocatorias/vigentes", "http://s3-postulacion-evaluacion:5103/api/v1/convocatorias/vigentes")]
    [InlineData("/api/v1/becas/123/desembolsos", "http://s4-becas:5104/api/v1/becas/123/desembolsos")]
    [InlineData("/api/v1/seguimiento/becarios", "http://s5-seguimiento:5105/api/v1/seguimiento/becarios")]
    [InlineData("/api/v1/reportes/impacto", "http://s6-comunicaciones:5106/api/v1/reportes/impacto")]
    [InlineData("/hubs/notificaciones", "http://s6-comunicaciones:5106/hubs/notificaciones")]
    public async Task Yarp_EnrutaPorPrefijoHaciaElMicroservicioCorrecto(string rutaEntrante, string destinoEsperado)
    {
        using var factory = new GatewayTestFactory();
        using var client = factory.CreateClient();

        var respuesta = await client.GetAsync(rutaEntrante);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var capturada = Assert.Single(factory.Handler.SolicitudesCapturadas);
        Assert.Equal(destinoEsperado, capturada.Uri);
    }

    [Fact]
    public async Task Yarp_PropagaAuthorizationActorYCorrelationIdAlMicroservicioDestino()
    {
        using var factory = new GatewayTestFactory();
        using var client = factory.CreateClient();

        const string token = "Bearer jwt-prueba-ascenso";
        const string actorId = "11111111-1111-1111-1111-111111111111";
        const string correlationId = "corr-propagacion-s2";

        using var peticion = new HttpRequestMessage(HttpMethod.Get, "/api/v1/donantes");
        peticion.Headers.Add("Authorization", token);
        peticion.Headers.Add("X-Actor-Usuario-Id", actorId);
        peticion.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        var respuesta = await client.SendAsync(peticion);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(correlationId, respuesta.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single());

        var capturada = Assert.Single(factory.Handler.SolicitudesCapturadas);
        Assert.Equal("http://s2-catalogo-fondos:5102/api/v1/donantes", capturada.Uri);
        Assert.Equal(token, capturada.Headers["Authorization"]);
        Assert.Equal(actorId, capturada.Headers["X-Actor-Usuario-Id"]);
        Assert.Equal(correlationId, capturada.Headers[CorrelationIdMiddleware.HeaderName]);
    }

    [Fact]
    public void Configuracion_NoDeclaraNingunaRutaInterna()
    {
        using var factory = new GatewayTestFactory();
        var config = factory.Services.GetRequiredService<IConfiguration>();
        var rutas = config.GetSection("ReverseProxy:Routes").GetChildren().ToList();

        Assert.NotEmpty(rutas);
        foreach (var ruta in rutas)
        {
            var path = ruta["Match:Path"] ?? string.Empty;
            Assert.False(
                FronteraPrivadaMiddleware.EsRutaPrivadaOInsegura(path),
                $"La ruta '{ruta.Key}' expone un patrón privado o inseguro: '{path}'.");
        }
    }

    private sealed record SolicitudCapturada(string Uri, Dictionary<string, string> Headers);

    private sealed class CapturandoHttpMessageHandler : HttpMessageHandler
    {
        public ConcurrentQueue<SolicitudCapturada> SolicitudesCapturadas { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var headers = request.Headers.ToDictionary(
                h => h.Key,
                h => string.Join(",", h.Value),
                StringComparer.OrdinalIgnoreCase);

            SolicitudesCapturadas.Enqueue(new SolicitudCapturada(
                request.RequestUri?.AbsoluteUri ?? string.Empty,
                headers));

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            };
            return Task.FromResult(response);
        }
    }

    private sealed class CapturandoForwarderHttpClientFactory(CapturandoHttpMessageHandler handler) : IForwarderHttpClientFactory
    {
        public HttpMessageInvoker CreateClient(ForwarderHttpClientContext context) =>
            new(handler, disposeHandler: false);
    }

    private sealed class GatewayTestFactory : WebApplicationFactory<Program>
    {
        public CapturandoHttpMessageHandler Handler { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Usa entorno Production para validar los destinos declarativos de appsettings.json (s1-identidad..s6-comunicaciones).
            builder.UseEnvironment("Production");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IForwarderHttpClientFactory>();
                services.AddSingleton<IForwarderHttpClientFactory>(new CapturandoForwarderHttpClientFactory(Handler));
            });
        }
    }
}
