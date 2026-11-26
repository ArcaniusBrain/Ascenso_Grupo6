using System.Text.Json;

namespace Ascenso.S2.CatalogoFondos.Infrastructure.Seguridad;

public enum EstadoAutorizacionInternaFondos
{
    AUTORIZADO = 1,
    NO_AUTENTICADO = 2,
    PROHIBIDO = 3
}

public sealed record ResultadoAutorizacionInternaFondos(
    EstadoAutorizacionInternaFondos Estado,
    string? Motivo = null);

/// <summary>
/// Protege las rutas privadas /internal/v1/fondos/... (contracts/openapi-interno.yaml, SEGURIDAD_INTERSERVICIOS_V1.1.md).
/// Exige token OAuth2 client_credentials con el scope exacto requerido y rechaza siempre tokens de usuario final.
/// </summary>
public sealed class ValidadorScopeInternoFondos
{
    public const string ScopeReservar = "fondos.reservar";
    public const string ScopeLiberar = "fondos.liberar";
    public const string ScopeConsultar = "fondos.consultar";

    public ResultadoAutorizacionInternaFondos ValidarAuthorizationHeader(
        string? authorizationHeader,
        string scopeRequerido)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoAutorizacionInternaFondos(
                EstadoAutorizacionInternaFondos.NO_AUTENTICADO,
                "Se requiere credencial Bearer de servicio interno.");
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var partes = token.Split('.');
        if (partes.Length != 3)
        {
            return new ResultadoAutorizacionInternaFondos(
                EstadoAutorizacionInternaFondos.NO_AUTENTICADO,
                "El token Bearer no tiene formato JWT válido.");
        }

        JsonDocument payload;
        try
        {
            var jsonBytes = Base64UrlDecode(partes[1]);
            payload = JsonDocument.Parse(jsonBytes);
        }
        catch
        {
            return new ResultadoAutorizacionInternaFondos(
                EstadoAutorizacionInternaFondos.NO_AUTENTICADO,
                "No se pudo interpretar el token Bearer.");
        }

        using (payload)
        {
            var root = payload.RootElement;

            // Un token de usuario final (postulante, becario o personal con sesión web) nunca puede invocar /internal/v1/fondos
            var tieneEmailUsuario = root.TryGetProperty("email", out var emailProp)
                && emailProp.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(emailProp.GetString());

            var tieneRolUsuario = root.TryGetProperty("role", out var roleProp)
                && roleProp.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(roleProp.GetString());

            if (tieneEmailUsuario || tieneRolUsuario)
            {
                return new ResultadoAutorizacionInternaFondos(
                    EstadoAutorizacionInternaFondos.PROHIBIDO,
                    "Un token de usuario final no está autorizado para operar en la frontera privada de fondos.");
            }

            if (!root.TryGetProperty("scope", out var scopeProp) || scopeProp.ValueKind != JsonValueKind.String)
            {
                return new ResultadoAutorizacionInternaFondos(
                    EstadoAutorizacionInternaFondos.PROHIBIDO,
                    $"El token de servicio no contiene el scope requerido '{scopeRequerido}'.");
            }

            var scopes = (scopeProp.GetString() ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (!scopes.Contains(scopeRequerido, StringComparer.Ordinal))
            {
                return new ResultadoAutorizacionInternaFondos(
                    EstadoAutorizacionInternaFondos.PROHIBIDO,
                    $"El token de servicio carece del scope requerido '{scopeRequerido}'.");
            }

            return new ResultadoAutorizacionInternaFondos(EstadoAutorizacionInternaFondos.AUTORIZADO);
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        var mod = padded.Length % 4;
        if (mod == 2) padded += "==";
        else if (mod == 3) padded += "=";
        return Convert.FromBase64String(padded);
    }
}
