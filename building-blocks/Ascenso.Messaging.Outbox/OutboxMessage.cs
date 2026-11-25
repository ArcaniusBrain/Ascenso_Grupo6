namespace Ascenso.Messaging.Outbox;

/// <summary>
/// Mensaje transaccional de salida (Outbox, ADR-12 y ADR-13).
/// Se guarda en la misma transacción local que el agregado y el RegistroDecision.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public int SchemaVersion { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public Guid CorrelationId { get; private set; }
    public Guid CausationId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    private OutboxMessage()
    {
    }

    public static OutboxMessage Crear(
        Guid eventId,
        string eventType,
        int schemaVersion,
        string payloadJson,
        Guid correlationId,
        Guid causationId,
        DateTimeOffset occurredAtUtc)
    {
        if (eventId == Guid.Empty) throw new ArgumentException("EventId obligatorio.", nameof(eventId));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("EventType obligatorio.", nameof(eventType));
        if (string.IsNullOrWhiteSpace(payloadJson)) throw new ArgumentException("PayloadJson obligatorio.", nameof(payloadJson));

        return new OutboxMessage
        {
            Id = eventId,
            EventType = eventType.Trim(),
            SchemaVersion = schemaVersion,
            PayloadJson = payloadJson,
            CorrelationId = correlationId,
            CausationId = causationId,
            OccurredAtUtc = occurredAtUtc,
            Attempts = 0
        };
    }

    public void MarcarProcesado(DateTimeOffset procesadoEnUtc)
    {
        ProcessedAtUtc = procesadoEnUtc;
        LastError = null;
    }

    public void RegistrarFallo(string error)
    {
        Attempts++;
        LastError = error.Length > 2000 ? error[..2000] : error;
    }
}
