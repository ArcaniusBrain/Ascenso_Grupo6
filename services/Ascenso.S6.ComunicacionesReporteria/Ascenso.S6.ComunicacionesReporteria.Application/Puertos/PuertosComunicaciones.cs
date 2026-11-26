using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S6.ComunicacionesReporteria.Application.Puertos;

public sealed record ActorSolicitudComunicaciones(
    Guid UsuarioId,
    string Rol,
    IReadOnlyCollection<Guid>? ProgramasAsignados = null);

/// <summary>
/// Política de autorización para RF-33 (HU-33.1 E7, HU-33.2 E3, C20).
/// </summary>
public static class PoliticaAutorizacionImpacto
{
    public static void ExigirPropioBecario(ActorSolicitudComunicaciones actor, Guid? becarioObjetivoId = null)
    {
        ValidarAutenticado(actor);

        var rol = (actor.Rol ?? string.Empty).Trim();
        if (!string.Equals(rol, "BECARIO", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "Solo el propio becario puede registrar su consentimiento para ser presentado a su donante.",
                "ACCESO_DENEGADO_CONSENTIMIENTO");
        }

        if (becarioObjetivoId.HasValue && becarioObjetivoId.Value != Guid.Empty && becarioObjetivoId.Value != actor.UsuarioId)
        {
            throw new ExcepcionDeDominio(
                "No puedes registrar consentimientos en nombre de otro becario.",
                "ACCESO_DENEGADO_CONSENTIMIENTO");
        }
    }

    public static void ExigirGeneracionReporte(ActorSolicitudComunicaciones actor)
    {
        ValidarAutenticado(actor);

        var rol = (actor.Rol ?? string.Empty).Trim();
        if (!string.Equals(rol, "DIRECCION", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "COORDINADOR", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "No tienes permiso para generar reportes de impacto a donantes.",
                "ACCESO_DENEGADO_REPORTES");
        }
    }

    public static void ExigirLecturaReporte(ActorSolicitudComunicaciones actor)
    {
        ValidarAutenticado(actor);

        var rol = (actor.Rol ?? string.Empty).Trim();
        if (!string.Equals(rol, "DIRECCION", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "COORDINADOR", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(rol, "FINANZAS", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "No tienes permiso para consultar reportes de impacto a donantes.",
                "ACCESO_DENEGADO_REPORTES");
        }
    }

    private static void ValidarAutenticado(ActorSolicitudComunicaciones actor)
    {
        if (actor is null || actor.UsuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Se requiere un usuario autenticado.", "NO_AUTENTICADO");
        }
    }
}

/// <summary>
/// Puerto CQRS de lectura sobre las proyecciones locales de S6 (s6_comunicaciones / MongoDB en despliegue).
/// Nunca consulta el schema de otros microservicios (ADR-05).
/// </summary>
public interface IRepositorioProyeccionImpacto
{
    Task<bool> ExistePeriodoEnCalendarioAsync(string codigoPeriodo, CancellationToken ct = default);

    Task<IReadOnlyList<AporteDonanteProyectado>> ObtenerAportesDonanteEnPeriodoAsync(
        Guid donanteId,
        string codigoPeriodo,
        CancellationToken ct = default);

    Task<IReadOnlyList<BecarioProyectadoEntrada>> ObtenerBecariosDeProgramasAsync(
        IReadOnlyCollection<Guid> programaIds,
        CancellationToken ct = default);

    Task<IReadOnlyList<DesembolsoProyectadoEntrada>> ObtenerDesembolsosEnPeriodoAsync(
        IReadOnlyCollection<Guid> programaIds,
        string codigoPeriodo,
        CancellationToken ct = default);

    Task<IReadOnlyList<ReporteAcademicoProyectadoEntrada>> ObtenerReportesAcademicosEnPeriodoAsync(
        IReadOnlyCollection<Guid> programaIds,
        string codigoPeriodo,
        CancellationToken ct = default);
}

/// <summary>
/// Cola asíncrona en segundo plano para generar reportes de impacto sin bloquear la operación diaria (HU-33.1 E9, E12).
/// </summary>
public interface IColaProcesamientoReportesImpacto
{
    ValueTask EncolarAsync(Guid reporteId, CancellationToken ct = default);
}
