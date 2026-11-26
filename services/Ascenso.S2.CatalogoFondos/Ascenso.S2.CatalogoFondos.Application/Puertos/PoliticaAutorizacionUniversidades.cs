using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Application.Puertos;

/// <summary>
/// Política de autorización para universidades aliadas, convenios, costos e inicios de inscripciones (HU-14.1, HU-14.2, HU-14.3, C16).
/// </summary>
public static class PoliticaAutorizacionUniversidades
{
    private static readonly HashSet<string> RolesGestion = new(StringComparer.OrdinalIgnoreCase)
    {
        "DIRECCION",
        "COORDINADOR"
    };

    public static void ExigirGestionUniversidad(ActorSolicitudCatalogo actor)
    {
        if (actor is null || actor.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(
                "Se requiere un usuario autenticado.",
                "NO_AUTENTICADO");
        }

        if (string.IsNullOrWhiteSpace(actor.Rol) || !RolesGestion.Contains(actor.Rol.Trim()))
        {
            throw new ExcepcionDeDominio(
                "No tienes permiso para gestionar universidades aliadas ni sus convenios.",
                "ACCESO_DENEGADO_UNIVERSIDADES");
        }
    }

    public static void ExigirUsuarioAutenticado(ActorSolicitudCatalogo actor)
    {
        if (actor is null || actor.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(
                "Se requiere un usuario autenticado.",
                "NO_AUTENTICADO");
        }
    }
}
