using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ascenso.Messaging.Outbox.RabbitMq;

/// <summary>
/// Worker reutilizable para cualquier microservicio (S1..S6) que despacha eventos
/// desde la tabla outbox_messages local hacia RabbitMQ (ADR-13).
/// Si RabbitMq:Host no está configurado, permanece inactivo sin afectar arranque ni pruebas.
/// </summary>
public sealed class OutboxDispatcherWorker<TDbContext> : BackgroundService
    where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly string _producerName;
    private readonly ILogger<OutboxDispatcherWorker<TDbContext>> _logger;

    public OutboxDispatcherWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        string producerName,
        ILogger<OutboxDispatcherWorker<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _producerName = producerName;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.EstaHabilitado)
        {
            _logger.LogInformation(
                "OutboxDispatcherWorker ({Producer}) inactivo porque RabbitMq:Host no está configurado.",
                _producerName);
            return;
        }

        var intervalo = TimeSpan.FromMilliseconds(Math.Clamp(_options.PollingIntervalMs, 250, 30000));
        _logger.LogInformation(
            "OutboxDispatcherWorker ({Producer}) iniciado contra RabbitMQ {Host}:{Port} exchange '{Exchange}'.",
            _producerName,
            _options.Host,
            _options.Port,
            _options.ExchangeName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var procesador = scope.ServiceProvider.GetRequiredService<ProcesadorOutboxLote<TDbContext>>();
                var resultado = await procesador.ProcesarLotePendienteAsync(
                    _producerName,
                    _options.BatchSize,
                    stoppingToken);

                if (resultado.Publicados > 0)
                {
                    _logger.LogInformation(
                        "Outbox ({Producer}): {Publicados} evento(s) publicado(s) a RabbitMQ ({Fallidos} fallo(s)).",
                        _producerName,
                        resultado.Publicados,
                        resultado.Fallidos);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Error en ciclo de despacho Outbox ({Producer}). Se reintentará en el siguiente intervalo.",
                    _producerName);
            }

            try
            {
                await Task.Delay(intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
