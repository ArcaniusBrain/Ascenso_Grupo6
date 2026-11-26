using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Application.Puertos;

/// <summary>
/// Política de autorización para donantes y aportes financieros (HU-13.1 E5, HU-13.2 E8, C18).
/// Dirección y Coordinador registran donantes y aportes (Coordinador acotado a sus programas si se informa alcance).
/// Dirección, Coordinador y Finanzas consultan donantes e historial de aportes.
/// Evaluador, Enlace universitario, Becario y Postulante quedan bloqueados.
/// </summary>
public static class PoliticaAutorizacionDonantes
{
    public static void ExigirGestionDonante(ActorSolicitudCatalogo actor)
    {
        ValidarAutenticado(actor);

        var rol = (actor.Rol ?? string.Empty).Trim();
        if (!string.Equals(rol, "DIRECCION", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "COORDINADOR", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "No tienes permiso para registrar donantes ni aportes.",
                "ACCESO_DENEGADO_DONANTES");
        }
    }

    public static void ExigirRegistroAporte(ActorSolicitudCatalogo actor, Guid programaId)
    {
        ExigirGestionDonante(actor);

        if (string.Equals(actor.Rol.Trim(), "DIRECCION", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (programaId != Guid.Empty
            && actor.ProgramasAsignados is { Count: > 0 } asignados
            && !asignados.Contains(programaId))
        {
            throw new ExcepcionDeDominio(
                "Solo puedes registrar aportes para los programas de becas que coordinas.",
                "ACCESO_DENEGADO_DONANTES");
        }
    }

    public static void ExigirLecturaHistorial(ActorSolicitudCatalogo actor)
    {
        ValidarAutenticado(actor);

        var rol = (actor.Rol ?? string.Empty).Trim();
        if (!string.Equals(rol, "DIRECCION", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "COORDINADOR", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "FINANZAS", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "No tienes permiso para consultar la información financiera de donantes y aportes.",
                "ACCESO_DENEGADO_DONANTES");
        }
    }

    private static void ValidarAutenticado(ActorSolicitudCatalogo actor)
    {
        if (actor is null || actor.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(
                "Se requiere un usuario autenticado.",
                "NO_AUTENTICADO");
        }
    }
}
