namespace Ascenso.Messaging.Outbox;

/// <summary>
/// Registro de consumo idempotente (Inbox, ADR-13).
/// La unicidad sobre (ConsumerName, EventId) garantiza entrega efectivamente única en la misma transacción que los efectos.
/// </summary>
public sealed class InboxMessage
{
    public string ConsumerName { get; private set; } = string.Empty;
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public DateTimeOffset ProcessedAtUtc { get; private set; }

    private InboxMessage()
    {
    }

    public static InboxMessage Crear(string consumerName, Guid eventId, string eventType, DateTimeOffset processedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(consumerName)) throw new ArgumentException("ConsumerName obligatorio.", nameof(consumerName));
        if (eventId == Guid.Empty) throw new ArgumentException("EventId obligatorio.", nameof(eventId));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("EventType obligatorio.", nameof(eventType));

        return new InboxMessage
        {
            ConsumerName = consumerName.Trim(),
            EventId = eventId,
            EventType = eventType.Trim(),
            ProcessedAtUtc = processedAtUtc
        };
    }
}
