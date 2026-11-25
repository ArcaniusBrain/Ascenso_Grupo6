namespace Ascenso.Contracts.Eventos;

public sealed record PeriodoMarcadoActualV1(
    Guid CalendarioId,
    string CodigoPeriodo,
    string? CodigoPeriodoAnterior);

public sealed record CostoMatriculaVigenteV1(
    Guid UniversidadId,
    string CodigoPeriodo,
    decimal Monto,
    string Moneda,
    int VersionRegistro,
    DateTimeOffset VigenteDesdeUtc);
