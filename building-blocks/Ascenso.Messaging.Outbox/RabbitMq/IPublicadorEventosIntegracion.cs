namespace Ascenso.Messaging.Outbox.RabbitMq;

/// <summary>
/// Mensaje listo para despacharse desde la tabla Outbox hacia el bus de integración (ADR-13).
/// </summary>
public sealed record EventoIntegracionMensaje(
    Guid EventId,
    string EventType,
    int SchemaVersion,
    string PayloadJson,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAtUtc,
    string Producer);

/// <summary>
/// Puerto de publicación de eventos de integración desde el Outbox hacia el broker (RabbitMQ en producción/Docker).
/// </summary>
public interface IPublicadorEventosIntegracion
{
    Task PublicarAsync(EventoIntegracionMensaje mensaje, CancellationToken ct = default);
}
