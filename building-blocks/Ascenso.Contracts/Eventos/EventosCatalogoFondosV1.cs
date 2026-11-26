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

public sealed record AporteRegistradoV1(
    Guid AporteId,
    Guid DonanteId,
    string NombreDonante,
    Guid ProgramaId,
    string CodigoPeriodo,
    decimal Monto,
    string Moneda,
    DateTimeOffset RegistradoEnUtc);

public sealed record CupoLiberadoV1(
    Guid ProgramaId,
    Guid? CicloId,
    Guid ReservaId,
    string Motivo,
    decimal MontoLiberado,
    string Moneda,
    DateTimeOffset LiberadoEnUtc);
