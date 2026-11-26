namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Programas;

public sealed record DonanteAporteResumenDto(
    Guid DonanteId,
    string NombreDonante,
    decimal TotalAportado,
    string Moneda,
    string? LeyendaSinAportes);

public sealed record RestriccionElegibilidadDto(
    Guid UniversidadId,
    string NombreUniversidad,
    IReadOnlyList<string> Carreras,
    string? AvisoSinCarreras);

public sealed record CondicionesPermanenciaDto(
    decimal PromedioMinimo,
    bool ExigeHorasServicio,
    int? HorasServicio,
    string LeyendaHorasServicio,
    IReadOnlyList<string> ReportesExigidos,
    int DuracionMaxPausaMeses,
    string DescripcionRenovacion);

public sealed record ProgramaResumenDto(
    Guid Id,
    string Nombre,
    string PeriodoInicio,
    int CupoBecarios,
    int DuracionPeriodos,
    decimal MontoPorPeriodo,
    string Moneda,
    string ModalidadPago,
    string Alcance,
    bool PermiteAcumulacion,
    int Version);

public sealed record ProgramaDetalleDto(
    Guid Id,
    string Nombre,
    string PeriodoInicio,
    int CupoBecarios,
    int DuracionPeriodos,
    decimal MontoPorPeriodo,
    string Moneda,
    string ModalidadPago,
    string Alcance,
    bool PermiteAcumulacion,
    decimal PresupuestoTotal,
    IReadOnlyList<DonanteAporteResumenDto> Donantes,
    string? LeyendaElegibilidad,
    IReadOnlyList<RestriccionElegibilidadDto> Restricciones,
    CondicionesPermanenciaDto? Condiciones,
    int Version);

public sealed record RestriccionUniversidadInput(
    Guid UniversidadId,
    IReadOnlyList<string>? Carreras);
