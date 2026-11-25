using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S1.Identidad.Application.Puertos;

public sealed record ResultadoValidacionCredenciales(
    bool Exitoso,
    string? KeycloakUserId = null,
    string? AccessTokenPrimario = null,
    string? RefreshTokenPrimario = null);

public sealed record SesionAutenticadaDto(
    Guid UsuarioId,
    string Correo,
    string NombreCompleto,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permisos,
    bool SegundoFactorVerificado,
    Guid SelloSeguridad,
    string AccessToken,
    DateTimeOffset ExpiraEnUtc);

public sealed class ExcepcionEnvioCodigo : ExcepcionDeDominio
{
    public ExcepcionEnvioCodigo(string mensaje)
        : base(mensaje, "FALLO_ENVIO_SEGUNDO_FACTOR")
    {
    }
}

public sealed record ConfiguracionAutenticacion2Fa(
    int VigenciaMinutos = DesafioSegundoFactor.VigenciaMinutosPorDefecto,
    int HorasBloqueo = DesafioSegundoFactor.HorasBloqueoTrasTresFallos);

/// <summary>
/// Puerto de salida hacia Keycloak 26 OIDC / Admin API (ADR-09).
/// </summary>
public interface IProveedorIdentidadKeycloak
{
    Task<ResultadoValidacionCredenciales> ValidarCredencialesAsync(
        string correo,
        string contrasena,
        CancellationToken cancellationToken = default);

    Task<string> CrearUsuarioAsync(
        string correo,
        string nombreCompleto,
        string? contrasenaTemporal,
        CancellationToken cancellationToken = default);

    Task CerrarSesionesUsuarioAsync(
        string keycloakUserId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Puerto extensible de entrega del código 2FA por correo o SMS (ADR-20, HU-37.2).
/// </summary>
public interface ICanalEnvioCodigo
{
    Task EnviarCodigoSegundoFactorAsync(
        CanalCodigo canal,
        string destino,
        string codigo,
        int vigenciaMinutos,
        bool esRecuperacion,
        CancellationToken cancellationToken = default);
}

public interface IGeneradorCodigoSeguro
{
    string GenerarCodigoNumerico(int longitud = 6);
}

public interface IServicioSesionAutenticada
{
    Task<SesionAutenticadaDto> EmitirSesionAsync(
        Usuario usuario,
        IReadOnlyList<TipoUsuario> tipos,
        bool segundoFactorVerificado,
        string? accessTokenKeycloak,
        CancellationToken cancellationToken = default);
}
