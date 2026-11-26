using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Application.Puertos;

/// <summary>
/// Política de autorización para la vista pública de presupuesto y cupo de un programa (HU-12.1 E7, HU-12.2 E5, C19).
/// Solo DIRECCION, COORDINADOR y FINANZAS pueden consultar el presupuesto y cupo disponible.
/// BECARIO, POSTULANTE, EVALUADOR y ENLACE_UNIVERSIDAD reciben 403 Forbidden.
/// </summary>
public static class PoliticaAutorizacionPresupuesto
{
    public static void ExigirConsultaPresupuesto(ActorSolicitudCatalogo actor)
    {
        if (actor is null || actor.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(
                "Se requiere un usuario autenticado.",
                "NO_AUTENTICADO");
        }

        var rol = (actor.Rol ?? string.Empty).Trim();
        if (!string.Equals(rol, "DIRECCION", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "COORDINADOR", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "FINANZAS", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "No tienes permiso para consultar el presupuesto y el cupo disponible del programa.",
                "ACCESO_DENEGADO_PRESUPUESTO");
        }
    }
}
