using Microsoft.AspNetCore.Mvc;

namespace Ascenso.Gateway.Middleware;

/// <summary>
/// Defensa en profundidad en el borde de red (ADR-02, ADR-11, contracts/openapi-interno.yaml):
/// impide que cualquier solicitud entrante al API Gateway alcance rutas privadas `/internal/*`
/// incluso ante intentos de codificación o escape de ruta.
/// </summary>
public sealed class FronteraPrivadaMiddleware
{
    public const string CodigoBloqueo = "FRONTERA_PRIVADA_BLOQUEADA";
    private readonly RequestDelegate _next;

    public FronteraPrivadaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var rawPath = context.Request.Path.Value ?? string.Empty;
        var decodedPath = Uri.UnescapeDataString(rawPath);

        if (EsRutaPrivadaOInsegura(decodedPath))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json; charset=utf-8";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Frontera privada no expuesta",
                Detail = "Las rutas /internal/* son exclusivas de comunicación interna entre microservicios y nunca se exponen en el API Gateway.",
                Instance = rawPath
            };
            problem.Extensions["code"] = CodigoBloqueo;

            await context.Response.WriteAsJsonAsync(
                problem,
                options: null,
                contentType: "application/problem+json; charset=utf-8");
            return;
        }

        await _next(context);
    }

    public static bool EsRutaPrivadaOInsegura(string ruta)
    {
        if (string.IsNullOrEmpty(ruta))
        {
            return false;
        }

        if (ruta.Contains("..", StringComparison.Ordinal) || ruta.Contains('\\'))
        {
            return true;
        }

        var segmentos = ruta.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segmentos.Any(s => string.Equals(s, "internal", StringComparison.OrdinalIgnoreCase));
    }
}
