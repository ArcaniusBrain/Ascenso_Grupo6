using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.Trazabilidad;

/// <summary>
/// Autor de una decisión registrada (DDD v3.3 §14: Actor).
/// Representa tanto a un usuario autenticado como a un proceso automático del sistema.
/// </summary>
public sealed record ActorDecision
{
    public Guid? UsuarioId { get; }
    public string Rol { get; }
    public bool EsSistema { get; }

    private ActorDecision(Guid? usuarioId, string rol, bool esSistema)
    {
        UsuarioId = usuarioId;
        Rol = rol;
        EsSistema = esSistema;
    }

    public static ActorDecision Usuario(Guid usuarioId, string rol)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador del usuario autor es obligatorio.", "ACTOR_USUARIO_INVALIDO");
        }

        var rolLimpio = (rol ?? string.Empty).Trim();
        if (rolLimpio.Length == 0)
        {
            throw new ExcepcionDeDominio("El rol del autor es obligatorio.", "ACTOR_ROL_OBLIGATORIO");
        }

        return new ActorDecision(usuarioId, rolLimpio, esSistema: false);
    }

    public static ActorDecision Sistema(string rolSistema = "SISTEMA")
    {
        var rolLimpio = string.IsNullOrWhiteSpace(rolSistema) ? "SISTEMA" : rolSistema.Trim();
        return new ActorDecision(usuarioId: null, rolLimpio, esSistema: true);
    }
}
