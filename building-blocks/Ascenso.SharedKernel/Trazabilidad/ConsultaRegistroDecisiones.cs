namespace Ascenso.SharedKernel.Trazabilidad;

/// <summary>
/// Filtros de consulta de solo lectura sobre el registro de decisiones (HU-35.2).
/// </summary>
public sealed record ConsultaRegistroDecisionesFiltro(
    TipoSujeto? TipoSujeto = null,
    string? SujetoId = null,
    Guid? ProgramaId = null,
    DateTimeOffset? DesdeUtc = null,
    DateTimeOffset? HastaUtc = null,
    int Pagina = 1,
    int TamanoPagina = 50);

/// <summary>
/// Vista inmutable de solo lectura de una decisión registrada (HU-35.2, HU-28.1, HU-36.1).
/// </summary>
public sealed record DecisionProyectada(
    Guid Id,
    TipoDecision Tipo,
    TipoSujeto TipoSujeto,
    string SujetoId,
    Guid? ProgramaId,
    Guid? PropietarioUsuarioId,
    Guid? ActorUsuarioId,
    string ActorRol,
    bool ActorEsSistema,
    DateTimeOffset OcurridoEnUtc,
    string Justificacion,
    string? EstadoAnterior,
    string? EstadoNuevo,
    string ServicioOrigen,
    Guid CorrelationId,
    string? DatosJson);

/// <summary>
/// Resultado tipado de la consulta de decisiones con marca temporal de proyección (ConsultaResponse)
/// y mensaje explícito cuando el caso aún no tiene decisiones registradas (HU-35.2 Escenario 2).
/// </summary>
public sealed record ResultadoConsultaDecisiones(
    DateTimeOffset ActualizadoEnUtc,
    IReadOnlyList<DecisionProyectada> Items,
    int Total,
    string? MensajeVacio = null)
{
    public const string MensajeCasoSinDecisiones = "Este caso aún no tiene decisiones registradas.";

    public static ResultadoConsultaDecisiones Vacio(DateTimeOffset actualizadoEnUtc) =>
        new(actualizadoEnUtc, Array.Empty<DecisionProyectada>(), 0, MensajeCasoSinDecisiones);
}

/// <summary>
/// Puerto de lectura pura sobre el registro de decisiones (DDD v3.3 §18, HU-35.2).
/// Por diseño no expone ninguna operación de creación, actualización ni borrado.
/// </summary>
public interface IConsultaRegistroDecisiones
{
    Task<ResultadoConsultaDecisiones> ConsultarPorSujetoAsync(
        SujetoRef sujeto,
        ContextoSolicitanteDecision solicitante,
        CancellationToken cancellationToken = default);

    Task<ResultadoConsultaDecisiones> ConsultarAsync(
        ConsultaRegistroDecisionesFiltro filtro,
        ContextoSolicitanteDecision solicitante,
        CancellationToken cancellationToken = default);
}
