namespace Ascenso.Contracts.Eventos;

public sealed record ReporteAcademicoRegistradoV1(
    Guid ReporteId,
    Guid BecaId,
    string CodigoPeriodo,
    decimal Promedio,
    int MateriasAprobadas,
    int MateriasReprobadas,
    string EstadoMatriculaOriginal,
    string EstadoMatriculaClasificado,
    string OrigenReporte,
    bool Tardio);

public sealed record IncumplimientoDetectadoV1(
    Guid EvaluacionCumplimientoId,
    Guid ReporteId,
    Guid BecaId,
    string CodigoPeriodo,
    IReadOnlyList<string> CondicionesIncumplidas);
