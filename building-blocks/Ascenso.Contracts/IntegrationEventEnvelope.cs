namespace Ascenso.Contracts;

/// <summary>
/// Sobre común de todos los eventos de integración en ASCENSO (contracts/EVENTOS_E_INTERFACES.md, ADR-13).
/// </summary>
public sealed record IntegrationEventEnvelope<TPayload>(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    Guid AggregateId,
    DateTimeOffset OccurredAtUtc,
    string Producer,
    Guid CorrelationId,
    Guid CausationId,
    TPayload Payload)
    where TPayload : notnull;
