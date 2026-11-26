using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Application.Puertos;

/// <summary>
/// Identidad y rol efectivo del actor autenticado (resuelto desde JWT/YARP o cabeceras de prueba).
/// </summary>
public sealed record ActorSolicitudCatalogo(Guid UsuarioId, string Rol);

/// <summary>
/// Política de autorización para el calendario académico (HU-38.1 E8, HU-38.2 E5, Anexo B HU-38.1 #2, C15).
/// </summary>
public static class PoliticaAutorizacionCalendario
{
    private static readonly HashSet<string> RolesInternosLectura = new(StringComparer.OrdinalIgnoreCase)
    {
        "DIRECCION",
        "COORDINADOR",
        "FINANZAS",
        "EVALUADOR"
    };

    public static void ExigirDireccion(ActorSolicitudCatalogo actor)
    {
        if (actor is null || actor.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(
                "Se requiere un usuario autenticado.",
                "NO_AUTENTICADO");
        }

        if (!string.Equals(actor.Rol?.Trim(), "DIRECCION", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "Solo Dirección puede realizar esta operación sobre el calendario académico.",
                "ACCESO_DENEGADO_CALENDARIO");
        }
    }

    public static void ExigirPersonalInterno(ActorSolicitudCatalogo actor)
    {
        if (actor is null || actor.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(
                "Se requiere un usuario autenticado.",
                "NO_AUTENTICADO");
        }

        if (string.IsNullOrWhiteSpace(actor.Rol) || !RolesInternosLectura.Contains(actor.Rol.Trim()))
        {
            throw new ExcepcionDeDominio(
                "Solo el personal interno autorizado puede consultar el calendario académico.",
                "ACCESO_DENEGADO_CALENDARIO");
        }
    }
}
