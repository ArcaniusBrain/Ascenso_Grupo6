namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;

public sealed record PeriodoResumenDto(
    string Codigo,
    string Estado,
    bool EsActual,
    DateTimeOffset RegistradoEnUtc);

public sealed record ListaPeriodosDto(
    Guid CalendarioId,
    int Version,
    string? PeriodoActual,
    string? Mensaje,
    IReadOnlyList<PeriodoResumenDto> Periodos);

public sealed record InicioInscripcionesDto(
    Guid Id,
    Guid UniversidadId,
    string NombreUniversidad,
    string CodigoPeriodo,
    DateOnly FechaInicio,
    bool InscripcionesIniciadas);

public sealed record PeriodoDetalleDto(
    string Codigo,
    string Estado,
    bool EsActual,
    bool EsPeriodoAnterior,
    DateTimeOffset RegistradoEnUtc,
    int CantidadElementosLigados,
    string? MensajeSinElementos,
    IReadOnlyList<InicioInscripcionesDto> IniciosInscripciones);

/// <summary>
/// Respuesta tipada compatible con contracts/openapi.yaml#/components/schemas/OperacionConfirmada.
/// </summary>
public sealed record OperacionConfirmadaDto(
    Guid Id,
    string Estado,
    Guid CorrelationId,
    int Version);
