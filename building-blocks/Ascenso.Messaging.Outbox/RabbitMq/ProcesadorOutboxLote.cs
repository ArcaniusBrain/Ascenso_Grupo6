using Ascenso.SharedKernel.Tiempo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ascenso.Messaging.Outbox.RabbitMq;

public sealed record ResultadoLoteOutbox(int Publicados, int Fallidos);

/// <summary>
/// Despachador de un lote de mensajes Outbox pendientes hacia IPublicadorEventosIntegracion (ADR-13).
/// Separado del BackgroundService para pruebas unitarias deterministas sin broker real.
/// </summary>
public sealed class ProcesadorOutboxLote<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _db;
    private readonly IPublicadorEventosIntegracion _publicador;
    private readonly IRelojNegocio _reloj;
    private readonly ILogger<ProcesadorOutboxLote<TDbContext>> _logger;

    public ProcesadorOutboxLote(
        TDbContext db,
        IPublicadorEventosIntegracion publicador,
        IRelojNegocio reloj,
        ILogger<ProcesadorOutboxLote<TDbContext>> logger)
    {
        _db = db;
        _publicador = publicador;
        _reloj = reloj;
        _logger = logger;
    }

    public async Task<ResultadoLoteOutbox> ProcesarLotePendienteAsync(
        string producerName,
        int batchSize = 50,
        CancellationToken ct = default)
    {
        var tamano = Math.Clamp(batchSize, 1, 500);

        // En SQLite los DateTimeOffset se ordenan en memoria para evitar limitaciones de ORDER BY sobre TEXT.
        var pendientes = await _db.Set<OutboxMessage>()
            .Where(m => m.ProcessedAtUtc == null)
            .ToListAsync(ct);

        var lote = pendientes
            .OrderBy(m => m.OccurredAtUtc)
            .Take(tamano)
            .ToList();

        if (lote.Count == 0)
        {
            return new ResultadoLoteOutbox(0, 0);
        }

        var publicados = 0;
        var fallidos = 0;

        foreach (var mensaje in lote)
        {
            try
            {
                await _publicador.PublicarAsync(
                    new EventoIntegracionMensaje(
                        mensaje.Id,
                        mensaje.EventType,
                        mensaje.SchemaVersion,
                        mensaje.PayloadJson,
                        mensaje.CorrelationId,
                        mensaje.CausationId,
                        mensaje.OccurredAtUtc,
                        producerName),
                    ct);

                mensaje.MarcarProcesado(_reloj.AhoraUtc);
                publicados++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                mensaje.RegistrarFallo(ex.Message);
                fallidos++;
                _logger.LogWarning(
                    ex,
                    "Error al publicar mensaje Outbox {EventId} ({EventType}) desde {Producer}. Intento #{Attempts}.",
                    mensaje.Id,
                    mensaje.EventType,
                    producerName,
                    mensaje.Attempts);
            }
        }

        await _db.SaveChangesAsync(ct);
        return new ResultadoLoteOutbox(publicados, fallidos);
    }
}
