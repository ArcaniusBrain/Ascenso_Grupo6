using Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;

namespace Ascenso.S6.ComunicacionesReporteria.Domain.Repositorios;

public interface IConsentimientoDonanteRepository
{
    Task<ConsentimientoDonante?> ObtenerPorBecarioYDonanteAsync(
        Guid becarioId,
        Guid donanteId,
        CancellationToken ct = default);

    Task<IReadOnlyList<ConsentimientoDonante>> ListarPorBecarioAsync(
        Guid becarioId,
        CancellationToken ct = default);

    Task<IReadOnlySet<Guid>> ObtenerBecariosConConsentimientoAsync(
        Guid donanteId,
        CancellationToken ct = default);

    Task AgregarAsync(ConsentimientoDonante consentimiento, CancellationToken ct = default);
}

public interface ISolicitudReporteImpactoRepository
{
    Task<SolicitudReporteImpacto?> ObtenerPorIdAsync(Guid reporteId, CancellationToken ct = default);

    Task<IReadOnlyList<SolicitudReporteImpacto>> ListarAsync(
        Guid? donanteId = null,
        string? codigoPeriodo = null,
        CancellationToken ct = default);

    Task AgregarAsync(SolicitudReporteImpacto solicitud, CancellationToken ct = default);
}

public interface IUnidadTrabajoComunicaciones
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
