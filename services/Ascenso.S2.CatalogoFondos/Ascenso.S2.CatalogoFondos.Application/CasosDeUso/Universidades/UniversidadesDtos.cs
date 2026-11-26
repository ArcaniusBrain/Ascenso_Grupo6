namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Universidades;

public sealed record ContactoDesignadoDto(
    string NombreCompleto,
    string? Correo,
    string? Telefono);

public sealed record CarreraElegibleDto(
    Guid Id,
    string Nombre);

public sealed record CostoMatriculaRegistroDto(
    Guid Id,
    string CodigoPeriodo,
    decimal Monto,
    string Moneda,
    int VersionRegistro,
    DateTimeOffset RegistradoEnUtc);

public sealed record CostoMatriculaPeriodoDto(
    Guid UniversidadId,
    string CodigoPeriodo,
    CostoMatriculaRegistroDto? CostoVigente,
    string? Mensaje,
    IReadOnlyList<CostoMatriculaRegistroDto> Historial);

public sealed record InicioInscripcionesUniversidadDto(
    Guid UniversidadId,
    string CodigoPeriodo,
    DateOnly? FechaInicio,
    bool InscripcionesIniciadas,
    string? Mensaje);

public sealed record UniversidadResumenDto(
    Guid Id,
    string Nombre,
    string TerminosConvenio,
    string ViaDatosAcademicos,
    ContactoDesignadoDto ContactoDesignado,
    int Version);

public sealed record UniversidadDetalleDto(
    Guid Id,
    string Nombre,
    string TerminosConvenio,
    string ViaDatosAcademicos,
    ContactoDesignadoDto ContactoDesignado,
    int Version,
    string? MensajeSinCarreras,
    IReadOnlyList<CarreraElegibleDto> Carreras,
    IReadOnlyList<CostoMatriculaRegistroDto> CostosVigentes);
