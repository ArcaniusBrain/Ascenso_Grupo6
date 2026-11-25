using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ascenso.S1.Identidad.Application.Puertos;
using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.Extensions.Options;

namespace Ascenso.S1.Identidad.Infrastructure.Seguridad;

public sealed class SesionTokenOptions
{
    public const string SectionName = "SesionToken";

    public string Issuer { get; set; } = "ascenso-s1-identidad";
    public string Audience { get; set; } = "ascenso-plataforma";
    public string SigningKey { get; set; } = string.Empty;
    public int DuracionMinutos { get; set; } = 60;
}

public sealed class GeneradorCodigoSeguroCripto : IGeneradorCodigoSeguro
{
    public string GenerarCodigoNumerico(int longitud = 6)
    {
        if (longitud is < 6 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(longitud), "La longitud debe estar entre 6 y 8 dígitos.");
        }

        var maximo = (int)Math.Pow(10, longitud);
        var valor = RandomNumberGenerator.GetInt32(0, maximo);
        return valor.ToString($"D{longitud}");
    }
}

/// <summary>
/// Emite la sesión verificada (JWT HS256 con claims de rol, permisos, amr y sello_seguridad)
/// únicamente después de haber superado la compuerta 2FA en S1 (ADR-09, HU-37.2, C14).
/// </summary>
public sealed class ServicioSesionAutenticadaJwt(
    IRelojNegocio reloj,
    IOptions<SesionTokenOptions> options) : IServicioSesionAutenticada
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<SesionAutenticadaDto> EmitirSesionAsync(
        Usuario usuario,
        IReadOnlyList<TipoUsuario> tipos,
        bool segundoFactorVerificado,
        string? accessTokenKeycloak,
        CancellationToken cancellationToken = default)
    {
        var cfg = options.Value;
        var ahora = reloj.AhoraUtc;
        var expira = ahora.AddMinutes(cfg.DuracionMinutos > 0 ? cfg.DuracionMinutos : 60);

        var roles = tipos.Select(t => t.Nombre).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray();
        var permisos = tipos.SelectMany(t => t.Permisos).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToArray();

        var token = GenerarJwtFirmado(usuario, roles, permisos, segundoFactorVerificado, ahora, expira, cfg);

        var dto = new SesionAutenticadaDto(
            UsuarioId: usuario.Id,
            Correo: usuario.Contacto.Correo,
            NombreCompleto: usuario.NombreCompleto,
            Roles: roles,
            Permisos: permisos,
            SegundoFactorVerificado: segundoFactorVerificado,
            SelloSeguridad: usuario.SelloSeguridad,
            AccessToken: token,
            ExpiraEnUtc: expira);

        return Task.FromResult(dto);
    }

    private static string GenerarJwtFirmado(
        Usuario usuario,
        string[] roles,
        string[] permisos,
        bool segundoFactorVerificado,
        DateTimeOffset ahora,
        DateTimeOffset expira,
        SesionTokenOptions cfg)
    {
        var headerJson = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" }, JsonOptions);
        var payloadJson = JsonSerializer.Serialize(new
        {
            sub = usuario.Id.ToString("D"),
            email = usuario.Contacto.Correo,
            name = usuario.NombreCompleto,
            iss = cfg.Issuer,
            aud = cfg.Audience,
            iat = ahora.ToUnixTimeSeconds(),
            exp = expira.ToUnixTimeSeconds(),
            sello_seguridad = usuario.SelloSeguridad.ToString("D"),
            amr = segundoFactorVerificado ? new[] { "pwd", "mfa" } : new[] { "pwd" },
            roles,
            permisos
        }, JsonOptions);

        var headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        var contenido = $"{headerB64}.{payloadB64}";

        var clave = string.IsNullOrWhiteSpace(cfg.SigningKey)
            ? "ascenso-dev-only-signing-key-replace-via-env-var-min-32-bytes"
            : cfg.SigningKey;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clave));
        var firma = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(contenido)));
        return $"{contenido}.{firma}";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
