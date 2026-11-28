namespace Ascenso.Gateway.Middleware;

/// <summary>
/// Garantiza que toda solicitud que entra por YARP cuente con un X-Correlation-Id
/// propagado tanto al microservicio destino como en la respuesta al cliente.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existente)
            && !string.IsNullOrWhiteSpace(existente.ToString())
                ? existente.ToString().Trim()
                : Guid.NewGuid().ToString();

        context.Request.Headers[HeaderName] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
