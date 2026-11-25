using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.SharedKernel.TiposBase;
using Microsoft.Extensions.Options;

namespace Ascenso.S1.Identidad.Infrastructure.Keycloak;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>
    /// URL base de Keycloak (inyectada desde IConfiguration / env vars, sin hardcodear localhost).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    public string Realm { get; set; } = "ascenso";
    public string ClientId { get; set; } = "ascenso-s1-identidad";
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Adaptador HTTP real contra Keycloak 26 OIDC Token Endpoint y Admin REST API (ADR-09).
/// </summary>
public sealed class ProveedorIdentidadKeycloakHttp(
    HttpClient httpClient,
    IOptions<KeycloakOptions> options) : IProveedorIdentidadKeycloak
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ResultadoValidacionCredenciales> ValidarCredencialesAsync(
        string correo,
        string contrasena,
        CancellationToken cancellationToken = default)
    {
        var cfg = options.Value;
        ValidarConfiguracion(cfg);

        var tokenUrl = $"{cfg.BaseUrl.TrimEnd('/')}/realms/{cfg.Realm}/protocol/openid-connect/token";
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = cfg.ClientId,
            ["username"] = correo,
            ["password"] = contrasena
        };

        if (!string.IsNullOrWhiteSpace(cfg.ClientSecret))
        {
            form["client_secret"] = cfg.ClientSecret;
        }

        using var respuesta = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(form), cancellationToken);
        if (respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            return new ResultadoValidacionCredenciales(Exitoso: false);
        }

        if (!respuesta.IsSuccessStatusCode)
        {
            throw new ExcepcionDeDominio(
                "El servicio de identidad no está disponible en este momento.",
                "KEYCLOAK_NO_DISPONIBLE");
        }

        var cuerpo = await respuesta.Content.ReadFromJsonAsync<KeycloakTokenResponse>(JsonOptions, cancellationToken);
        if (cuerpo is null || string.IsNullOrWhiteSpace(cuerpo.AccessToken))
        {
            return new ResultadoValidacionCredenciales(Exitoso: false);
        }

        return new ResultadoValidacionCredenciales(
            Exitoso: true,
            KeycloakUserId: null,
            AccessTokenPrimario: cuerpo.AccessToken,
            RefreshTokenPrimario: cuerpo.RefreshToken);
    }

    public async Task<string> CrearUsuarioAsync(
        string correo,
        string nombreCompleto,
        string? contrasenaTemporal,
        CancellationToken cancellationToken = default)
    {
        var cfg = options.Value;
        ValidarConfiguracion(cfg);

        var adminToken = await ObtenerTokenServicioAsync(cfg, cancellationToken);
        var usersUrl = $"{cfg.BaseUrl.TrimEnd('/')}/admin/realms/{cfg.Realm}/users";

        var partes = nombreCompleto.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = partes.Length > 0 ? partes[0] : nombreCompleto;
        var lastName = partes.Length > 1 ? partes[1] : string.Empty;

        var payload = new
        {
            username = correo,
            email = correo,
            firstName,
            lastName,
            enabled = true,
            emailVerified = true,
            credentials = string.IsNullOrWhiteSpace(contrasenaTemporal)
                ? Array.Empty<object>()
                : new object[]
                {
                    new { type = "password", value = contrasenaTemporal, temporary = false }
                }
        };

        using var solicitud = new HttpRequestMessage(HttpMethod.Post, usersUrl)
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var respuesta = await httpClient.SendAsync(solicitud, cancellationToken);
        if (!respuesta.IsSuccessStatusCode)
        {
            throw new ExcepcionDeDominio(
                "No se pudo aprovisionar el usuario en Keycloak.",
                "KEYCLOAK_CREAR_USUARIO_ERROR");
        }

        var location = respuesta.Headers.Location?.ToString();
        if (!string.IsNullOrWhiteSpace(location))
        {
            var segmentos = location.TrimEnd('/').Split('/');
            return segmentos[^1];
        }

        return Guid.NewGuid().ToString("D");
    }

    public async Task CerrarSesionesUsuarioAsync(string keycloakUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
        {
            return;
        }

        var cfg = options.Value;
        ValidarConfiguracion(cfg);

        var adminToken = await ObtenerTokenServicioAsync(cfg, cancellationToken);
        var logoutUrl = $"{cfg.BaseUrl.TrimEnd('/')}/admin/realms/{cfg.Realm}/users/{Uri.EscapeDataString(keycloakUserId)}/logout";

        using var solicitud = new HttpRequestMessage(HttpMethod.Post, logoutUrl);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var respuesta = await httpClient.SendAsync(solicitud, cancellationToken);
        if (!respuesta.IsSuccessStatusCode && respuesta.StatusCode != HttpStatusCode.NotFound)
        {
            throw new ExcepcionDeDominio(
                "No se pudieron revocar las sesiones activas del usuario en Keycloak.",
                "KEYCLOAK_LOGOUT_ERROR");
        }
    }

    private async Task<string> ObtenerTokenServicioAsync(KeycloakOptions cfg, CancellationToken cancellationToken)
    {
        var tokenUrl = $"{cfg.BaseUrl.TrimEnd('/')}/realms/{cfg.Realm}/protocol/openid-connect/token";
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = cfg.ClientId,
            ["client_secret"] = cfg.ClientSecret
        };

        using var respuesta = await httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(form), cancellationToken);
        if (!respuesta.IsSuccessStatusCode)
        {
            throw new ExcepcionDeDominio(
                "No se pudo autenticar el cliente de servicio S1 contra Keycloak.",
                "KEYCLOAK_SERVICE_TOKEN_ERROR");
        }

        var cuerpo = await respuesta.Content.ReadFromJsonAsync<KeycloakTokenResponse>(JsonOptions, cancellationToken);
        return cuerpo?.AccessToken
            ?? throw new ExcepcionDeDominio("Keycloak no devolvió token de servicio.", "KEYCLOAK_SERVICE_TOKEN_ERROR");
    }

    private static void ValidarConfiguracion(KeycloakOptions cfg)
    {
        if (string.IsNullOrWhiteSpace(cfg.BaseUrl) || string.IsNullOrWhiteSpace(cfg.Realm))
        {
            throw new ExcepcionDeDominio(
                "La configuración de Keycloak (BaseUrl/Realm) no está definida.",
                "KEYCLOAK_CONFIG_INVALIDA");
        }
    }

    private sealed class KeycloakTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
    }
}
