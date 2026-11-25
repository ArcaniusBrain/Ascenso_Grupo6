using System.Text;
using System.Text.Json;

namespace Ascenso.S1.Identidad.Infrastructure.Seguridad;

public enum EstadoAutorizacionInterna
{
    AUTORIZADO = 1,
    NO_AUTENTICADO = 2,
    PROHIBIDO = 3
}

public sealed record ResultadoAutorizacionInterna(EstadoAutorizacionInterna Estado, string? Motivo = null);

/// <summary>
/// Protege la frontera privada /internal/v1/otp/validar-y-consumir (SEGURIDAD_INTERSERVICIOS_V1.1.md, ADR-10).
/// Exige token OAuth2 client_credentials con scope exacto "otp.consumir" y rechaza tokens de usuario final.
/// </summary>
public sealed class ValidadorScopeInternoOtp
{
    public const string ScopeRequerido = "otp.consumir";

    public ResultadoAutorizacionInterna ValidarAuthorizationHeader(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return new ResultadoAutorizacionInterna(
                EstadoAutorizacionInterna.NO_AUTENTICADO,
                "Se requiere credencial Bearer de servicio interno.");
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var partes = token.Split('.');
        if (partes.Length != 3)
        {
            return new ResultadoAutorizacionInterna(
                EstadoAutorizacionInterna.NO_AUTENTICADO,
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
            return new ResultadoAutorizacionInterna(
                EstadoAutorizacionInterna.NO_AUTENTICADO,
                "No se pudo interpretar el token Bearer.");
        }

        using (payload)
        {
            var root = payload.RootElement;

            // Un token de usuario final (postulante, becario o personal con sesión web) jamás puede consumir OTP en /internal/v1
            var esTokenUsuarioFinal = root.TryGetProperty("email", out var emailProp)
                && emailProp.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(emailProp.GetString());

            if (esTokenUsuarioFinal)
            {
                return new ResultadoAutorizacionInterna(
                    EstadoAutorizacionInterna.PROHIBIDO,
                    "Un token de usuario final no está autorizado para consumir OTP en la frontera privada.");
            }

            if (!root.TryGetProperty("scope", out var scopeProp) || scopeProp.ValueKind != JsonValueKind.String)
            {
                return new ResultadoAutorizacionInterna(
                    EstadoAutorizacionInterna.PROHIBIDO,
                    "El token de servicio no contiene el scope requerido otp.consumir.");
            }

            var scopes = (scopeProp.GetString() ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (!scopes.Contains(ScopeRequerido, StringComparer.Ordinal))
            {
                return new ResultadoAutorizacionInterna(
                    EstadoAutorizacionInterna.PROHIBIDO,
                    "El token de servicio carece del scope otp.consumir.");
            }

            return new ResultadoAutorizacionInterna(EstadoAutorizacionInterna.AUTORIZADO);
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
