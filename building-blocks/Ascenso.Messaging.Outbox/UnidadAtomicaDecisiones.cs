using System.Text.Json;
using Ascenso.Contracts;
using Ascenso.Contracts.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.Trazabilidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ascenso.Messaging.Outbox;

/// <summary>
/// Excepción lanzada cuando la persistencia del RegistroDecision falla dentro de la transacción atómica (HU-35.1 Escenario 3).
/// </summary>
public sealed class ExcepcionPersistenciaDecision : ExcepcionDeDominio
{
    public Exception? CausaTecnica { get; }

    public ExcepcionPersistenciaDecision(Exception? causaTecnica = null)
        : base(RegistroDecision.MensajeFalloPersistencia, "FALLO_PERSISTENCIA_DECISION")
    {
        CausaTecnica = causaTecnica;
    }
}

/// <summary>
/// Coordina la transacción atómica local: Agregado (con Version++ ADR-25) + RegistroDecision + OutboxMessage (D-6, ADR-12).
/// O se confirma todo junto o se revierte todo sin dejar cambios de estado sin registro ni registros huérfanos.
/// </summary>
public static class UnidadAtomicaDecisiones
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<RegistroDecision> ConfirmarAsync(
        DbContext dbContext,
        Func<CancellationToken, Task<RegistroDecision>> ejecutarOperacionDeDominio,
        IEnumerable<OutboxMessage>? mensajesAdicionales = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(ejecutarOperacionDeDominio);

        IDbContextTransaction? transaccion = null;
        var transaccionPropia = false;

        if (dbContext.Database.IsRelational() && dbContext.Database.CurrentTransaction is null)
        {
            transaccion = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            transaccionPropia = true;
        }

        try
        {
            var registro = await ejecutarOperacionDeDominio(cancellationToken);
            ArgumentNullException.ThrowIfNull(registro);

            dbContext.Set<RegistroDecision>().Add(registro);

            var outboxDecision = CrearOutboxParaDecision(registro);
            dbContext.Set<OutboxMessage>().Add(outboxDecision);

            if (mensajesAdicionales is not null)
            {
                foreach (var mensaje in mensajesAdicionales)
                {
                    dbContext.Set<OutboxMessage>().Add(mensaje);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            if (transaccionPropia && transaccion is not null)
            {
                await transaccion.CommitAsync(cancellationToken);
            }

            return registro;
        }
        catch (DbUpdateConcurrencyException)
        {
            await RevertirSeguroAsync(dbContext, transaccion, transaccionPropia, cancellationToken);
            throw;
        }
        catch (ExcepcionDeDominio)
        {
            await RevertirSeguroAsync(dbContext, transaccion, transaccionPropia, cancellationToken);
            throw;
        }
        catch (Exception ex)
        {
            await RevertirSeguroAsync(dbContext, transaccion, transaccionPropia, cancellationToken);
            throw new ExcepcionPersistenciaDecision(ex);
        }
        finally
        {
            if (transaccionPropia && transaccion is not null)
            {
                await transaccion.DisposeAsync();
            }
        }
    }

    public static OutboxMessage CrearOutboxParaDecision(RegistroDecision registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        var payload = new DecisionRegistradaV1(
            RegistroDecisionId: registro.Id,
            TipoDecision: registro.Tipo.ToString(),
            TipoSujeto: registro.Sujeto.Tipo.ToString(),
            SujetoId: registro.Sujeto.Id,
            ProgramaId: registro.ProgramaId,
            PropietarioUsuarioId: registro.PropietarioUsuarioId,
            ActorUsuarioId: registro.Actor.UsuarioId,
            ActorRol: registro.Actor.Rol,
            ActorEsSistema: registro.Actor.EsSistema,
            OcurridoEnUtc: registro.OcurridoEnUtc,
            Justificacion: registro.Justificacion.Texto,
            EstadoAnterior: registro.EstadoAnterior,
            EstadoNuevo: registro.EstadoNuevo,
            ServicioOrigen: registro.ServicioOrigen,
            DatosResumenJson: registro.DatosJson);

        var aggregateId = Guid.TryParse(registro.Sujeto.Id, out var parsed)
            ? parsed
            : registro.Id;

        var sobre = new IntegrationEventEnvelope<DecisionRegistradaV1>(
            EventId: registro.Id,
            EventType: nameof(DecisionRegistradaV1),
            SchemaVersion: 1,
            AggregateId: aggregateId,
            OccurredAtUtc: registro.OcurridoEnUtc,
            Producer: registro.ServicioOrigen,
            CorrelationId: registro.CorrelationId,
            CausationId: registro.Id,
            Payload: payload);

        var json = JsonSerializer.Serialize(sobre, JsonOptions);

        return OutboxMessage.Crear(
            eventId: sobre.EventId,
            eventType: sobre.EventType,
            schemaVersion: sobre.SchemaVersion,
            payloadJson: json,
            correlationId: sobre.CorrelationId,
            causationId: sobre.CausationId,
            occurredAtUtc: sobre.OccurredAtUtc);
    }

    private static async Task RevertirSeguroAsync(
        DbContext dbContext,
        IDbContextTransaction? transaccion,
        bool transaccionPropia,
        CancellationToken cancellationToken)
    {
        if (transaccionPropia && transaccion is not null)
        {
            await transaccion.RollbackAsync(cancellationToken);
        }

        dbContext.ChangeTracker.Clear();
    }
}
