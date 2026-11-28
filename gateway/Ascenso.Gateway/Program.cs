using Ascenso.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 1. Trazabilidad X-Correlation-Id en todas las peticiones y respuestas.
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Bloqueo estricto de fronteras privadas (/internal/*) antes de cualquier enrutamiento (ADR-02, ADR-11).
app.UseMiddleware<FronteraPrivadaMiddleware>();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Ascenso.Gateway"
}));

// 3. Proxy inverso YARP hacia S1..S6 (incluye soporte nativo de WebSockets para SignalR en /hubs/*, ADR-15).
app.MapReverseProxy();

app.Run();

public partial class Program;
