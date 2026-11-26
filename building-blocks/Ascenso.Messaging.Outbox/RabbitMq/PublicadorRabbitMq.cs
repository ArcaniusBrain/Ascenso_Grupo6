using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ascenso.Messaging.Outbox.RabbitMq;

/// <summary>
/// Implementación oficial sobre RabbitMQ.Client v7 que publica mensajes persistentes
/// en un exchange topic durable usando EventType como routing key (ADR-13).
/// </summary>
public sealed class PublicadorRabbitMq : IPublicadorEventosIntegracion, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public PublicadorRabbitMq(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublicarAsync(EventoIntegracionMensaje mensaje, CancellationToken ct = default)
    {
        if (!_options.EstaHabilitado)
        {
            throw new InvalidOperationException("RabbitMQ no está configurado (RabbitMq:Host vacío).");
        }

        await _lock.WaitAsync(ct);
        try
        {
            var channel = await AsegurarCanalAsync(ct);
            var body = Encoding.UTF8.GetBytes(mensaje.PayloadJson);

            var props = new BasicProperties
            {
                MessageId = mensaje.EventId.ToString(),
                Type = mensaje.EventType,
                CorrelationId = mensaje.CorrelationId.ToString(),
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(mensaje.OccurredAtUtc.ToUnixTimeSeconds()),
                Headers = new Dictionary<string, object?>
                {
                    ["x-schema-version"] = mensaje.SchemaVersion,
                    ["x-causation-id"] = mensaje.CausationId.ToString(),
                    ["x-producer"] = mensaje.Producer
                }
            };

            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: mensaje.EventType,
                mandatory: false,
                basicProperties: props,
                body: body,
                cancellationToken: ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IChannel> AsegurarCanalAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
        {
            return _channel;
        }

        await CerrarRecursosInternosAsync();

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            VirtualHost = string.IsNullOrWhiteSpace(_options.VirtualHost) ? "/" : _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            AutomaticRecoveryEnabled = true
        };

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        return _channel;
    }

    private async Task CerrarRecursosInternosAsync()
    {
        if (_channel is not null)
        {
            try { await _channel.DisposeAsync(); } catch { }
            _channel = null;
        }

        if (_connection is not null)
        {
            try { await _connection.DisposeAsync(); } catch { }
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _lock.WaitAsync();
        try
        {
            await CerrarRecursosInternosAsync();
        }
        finally
        {
            _lock.Release();
            _lock.Dispose();
        }
    }
}
