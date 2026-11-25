namespace Ascenso.Contracts.Eventos;

/// <summary>
/// Publicado por S1 al registrar una cuenta de usuario; S3 crea el Postulante de forma idempotente (DDD-11, ADR-30).
/// </summary>
public sealed record UsuarioRegistradoV1(
    Guid UsuarioId,
    string Cedula,
    string Correo,
    string Telefono,
    string TipoUsuario);
