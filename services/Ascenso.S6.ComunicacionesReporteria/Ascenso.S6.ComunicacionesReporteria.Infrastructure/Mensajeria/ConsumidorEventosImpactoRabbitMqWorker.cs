using System.Text;
using Ascenso.Messaging.Outbox.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure.Mensajeria;

/// <summary>
/// Consumidor RabbitMQ de S6 que escucha el exchange topic `ascenso.eventos`
/// y actualiza las proyecciones CQRS locales de reportes de impacto de forma idempotente (ADR-13).
/// Si RabbitMq:Host está vacío, permanece inactivo para no afectar pruebas ni arranque sin broker.
/// </summary>
public sealed class ConsumidorEventosImpactoRabbitMqWorker : BackgroundService
{
    public const string NombreCola = "s6.proyeccion-impacto";

    private static readonly string[] RoutingKeys =
    [
        ProyectorEventosImpacto.EventoPeriodoMarcadoActual,
        ProyectorEventosImpacto.EventoAporteRegistrado,
        ProyectorEventosImpacto.EventoPagoConfirmado
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<ConsumidorEventosImpactoRabbitMqWorker> _logger;

    public ConsumidorEventosImpactoRabbitMqWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<ConsumidorEventosImpactoRabbitMqWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EstaHabilitado)
        {
            _logger.LogInformation(
                "ConsumidorEventosImpactoRabbitMqWorker inactivo porque RabbitMq:Host no está configurado.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            IConnection? connection = null;
            IChannel? channel = null;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    Port = _options.Port,
                    VirtualHost = string.IsNullOrWhiteSpace(_options.VirtualHost) ? "/" : _options.VirtualHost,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    AutomaticRecoveryEnabled = true
                };

                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.ExchangeDeclareAsync(
                    exchange: _options.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: NombreCola,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                foreach (var routingKey in RoutingKeys)
                {
                    await channel.QueueBindAsync(
                        queue: NombreCola,
                        exchange: _options.ExchangeName,
                        routingKey: routingKey,
                        arguments: null,
                        cancellationToken: stoppingToken);
                }

                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var payloadJson = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var eventType = !string.IsNullOrWhiteSpace(ea.BasicProperties.Type)
                        ? ea.BasicProperties.Type
                        : ea.RoutingKey;
                    var eventId = Guid.TryParse(ea.BasicProperties.MessageId, out var parsed)
                        ? parsed
                        : Guid.NewGuid();

                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var proyector = scope.ServiceProvider.GetRequiredService<ProyectorEventosImpacto>();
                        var resultado = await proyector.ProcesarEventoAsync(eventId, eventType, payloadJson, stoppingToken);

                        _logger.LogInformation(
                            "S6 RabbitMQ consumió {EventType} ({EventId}) -> {Resultado}.",
                            eventType,
                            eventId,
                            resultado);

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogWarning(
                            ex,
                            "Error procesando evento {EventType} ({EventId}) en S6. Se reencolará.",
                            eventType,
                            eventId);

                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: NombreCola,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "S6 escuchando cola RabbitMQ '{Cola}' en exchange '{Exchange}' ({Host}:{Port}).",
                    NombreCola,
                    _options.ExchangeName,
                    _options.Host,
                    _options.Port);

                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo conectar el consumidor RabbitMQ de S6. Reintentando en 5 segundos...");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            finally
            {
                if (channel is not null) { try { await channel.DisposeAsync(); } catch { } }
                if (connection is not null) { try { await connection.DisposeAsync(); } catch { } }
            }
        }
    }
}
