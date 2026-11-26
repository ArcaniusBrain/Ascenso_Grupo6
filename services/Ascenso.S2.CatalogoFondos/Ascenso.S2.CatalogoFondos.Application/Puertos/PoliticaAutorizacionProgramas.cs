using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Application.Puertos;

/// <summary>
/// Política de autorización para programas de becas (HU-11.1 E7, HU-11.2, C17).
/// Dirección actúa globalmente; Coordinador crea y gestiona sus programas asignados.
/// </summary>
public static class PoliticaAutorizacionProgramas
{
    public static void ExigirCreacion(ActorSolicitudCatalogo actor)
    {
        ValidarAutenticado(actor);

        var rol = (actor.Rol ?? string.Empty).Trim();
        if (!string.Equals(rol, "DIRECCION", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "COORDINADOR", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "No tienes permiso para crear o configurar programas de becas.",
                "ACCESO_DENEGADO_PROGRAMAS");
        }
    }

    public static void ExigirGestionPrograma(ActorSolicitudCatalogo actor, Guid programaId)
    {
        ExigirCreacion(actor);

        if (string.Equals(actor.Rol.Trim(), "DIRECCION", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Si el token/cabecera especifica los programas asignados del coordinador, restringir a su alcance (C17).
        if (actor.ProgramasAsignados is { Count: > 0 } asignados && !asignados.Contains(programaId))
        {
            throw new ExcepcionDeDominio(
                "Solo puedes configurar los programas de becas que coordinas.",
                "ACCESO_DENEGADO_PROGRAMAS");
        }
    }

    public static void ExigirLectura(ActorSolicitudCatalogo actor) => ValidarAutenticado(actor);

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

public sealed record DonanteRegistradoInfo(Guid Id, string Nombre);

public sealed record AportesDonanteEnPrograma(
    Guid DonanteId,
    string NombreDonante,
    decimal TotalAportado,
    string Moneda,
    int CantidadAportes);

/// <summary>
/// Puerto de Application para consultar donantes registrados y la suma de sus aportes al programa (HU-11.2, S8).
/// Se conectará directamente con el agregado Donante/Aporte en RF-13 dentro del mismo schema s2_catalogo_fondos.
/// </summary>
public interface IConsultorDonantesYAportes
{
    Task<bool> ExistenDonantesRegistradosAsync(CancellationToken ct = default);
    Task<DonanteRegistradoInfo?> ObtenerDonanteAsync(Guid donanteId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, AportesDonanteEnPrograma>> ObtenerAportesPorProgramaAsync(
        Guid programaId,
        CancellationToken ct = default);
}
