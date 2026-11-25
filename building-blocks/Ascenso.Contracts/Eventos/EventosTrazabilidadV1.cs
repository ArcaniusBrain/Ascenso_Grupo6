namespace Ascenso.Contracts.Eventos;

/// <summary>
/// Evento de integración v1 emitido vía Outbox en la misma transacción que cada RegistroDecision (ADR-12, D-6).
/// Permite a S6 proyectar de forma idempotente registro_decisiones (HU-35.2) y linea_tiempo_beca (HU-28.1)
/// sin leer schemas PostgreSQL ajenos (ADR-05, DDD-14).
/// No serializa credenciales, cuentas bancarias sin máscara ni bloques socioeconómicos crudos.
/// </summary>
public sealed record DecisionRegistradaV1(
    Guid RegistroDecisionId,
    string TipoDecision,
    string TipoSujeto,
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
    string? DatosResumenJson);
