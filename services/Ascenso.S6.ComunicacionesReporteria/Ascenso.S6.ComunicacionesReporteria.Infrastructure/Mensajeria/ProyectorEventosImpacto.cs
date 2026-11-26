using System.Text.Json;
using Ascenso.Contracts.Eventos;
using Ascenso.Messaging.Outbox;
using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;
using Ascenso.SharedKernel.Tiempo;
using Microsoft.EntityFrameworkCore;

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure.Mensajeria;

public enum ResultadoConsumoEvento
{
    PROCESADO = 1,
    DUPLICADO_OMITIDO = 2,
    EVENTO_NO_RELEVANTE = 3
}

/// <summary>
/// Proyector CQRS idempotente de S6 (ADR-07, ADR-13):
/// alimenta las tablas locales proyeccion_* de s6_comunicaciones a partir de eventos de integración
/// garantizando procesamiento efectivamente único mediante InboxMessage en la misma transacción.
/// </summary>
public sealed class ProyectorEventosImpacto
{
    public const string NombreConsumidor = "s6.proyeccion-impacto";
    public const string EventoPeriodoMarcadoActual = "PeriodoMarcadoActual.v1";
    public const string EventoAporteRegistrado = "AporteRegistrado.v1";
    public const string EventoPagoConfirmado = "PagoConfirmado.v1";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ComunicacionesDbContext _db;
    private readonly IRelojNegocio _reloj;

    public ProyectorEventosImpacto(ComunicacionesDbContext db, IRelojNegocio reloj)
    {
        _db = db;
        _reloj = reloj;
    }

    public async Task<ResultadoConsumoEvento> ProcesarEventoAsync(
        Guid eventId,
        string eventType,
        string payloadJson,
        CancellationToken ct = default)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("EventId obligatorio para idempotencia Inbox.", nameof(eventId));
        }

        var yaProcesado = await _db.InboxMessages
            .AsNoTracking()
            .AnyAsync(x => x.ConsumerName == NombreConsumidor && x.EventId == eventId, ct);

        if (yaProcesado)
        {
            return ResultadoConsumoEvento.DUPLICADO_OMITIDO;
        }

        switch (eventType.Trim())
        {
            case EventoPeriodoMarcadoActual:
                await ProyectarPeriodoMarcadoActualAsync(payloadJson, ct);
                break;

            case EventoAporteRegistrado:
                await ProyectarAporteRegistradoAsync(payloadJson, ct);
                break;

            case EventoPagoConfirmado:
                await ProyectarPagoConfirmadoAsync(payloadJson, ct);
                break;

            default:
                return ResultadoConsumoEvento.EVENTO_NO_RELEVANTE;
        }

        var inbox = InboxMessage.Crear(NombreConsumidor, eventId, eventType, _reloj.AhoraUtc);
        await _db.InboxMessages.AddAsync(inbox, ct);
        await _db.SaveChangesAsync(ct);

        return ResultadoConsumoEvento.PROCESADO;
    }

    private async Task ProyectarPeriodoMarcadoActualAsync(string payloadJson, CancellationToken ct)
    {
        var evento = JsonSerializer.Deserialize<PeriodoMarcadoActualV1>(payloadJson, JsonOptions)
            ?? throw new InvalidOperationException("No se pudo deserializar PeriodoMarcadoActualV1.");

        await AsegurarPeriodoEnProyeccionAsync(evento.CodigoPeriodo, ct);

        if (!string.IsNullOrWhiteSpace(evento.CodigoPeriodoAnterior))
        {
            await AsegurarPeriodoEnProyeccionAsync(evento.CodigoPeriodoAnterior, ct);
        }
    }

    private async Task ProyectarAporteRegistradoAsync(string payloadJson, CancellationToken ct)
    {
        var evento = JsonSerializer.Deserialize<AporteRegistradoV1>(payloadJson, JsonOptions)
            ?? throw new InvalidOperationException("No se pudo deserializar AporteRegistradoV1.");

        await AsegurarPeriodoEnProyeccionAsync(evento.CodigoPeriodo, ct);

        var yaExisteAporte = await _db.ProyeccionAportes
            .AsNoTracking()
            .AnyAsync(a => a.Id == evento.AporteId, ct);

        if (yaExisteAporte)
        {
            return;
        }

        // Si ya existe otro aporte proyectado del mismo programa, reutilizamos su nombre legible.
        var nombreProgramaExistente = await _db.ProyeccionAportes
            .AsNoTracking()
            .Where(a => a.ProgramaId == evento.ProgramaId)
            .Select(a => a.NombrePrograma)
            .FirstOrDefaultAsync(ct);

        var nombrePrograma = !string.IsNullOrWhiteSpace(nombreProgramaExistente)
            ? nombreProgramaExistente
            : $"Programa {evento.ProgramaId.ToString()[..8]}";

        await _db.ProyeccionAportes.AddAsync(
            new AporteImpactoProyeccion
            {
                Id = evento.AporteId,
                DonanteId = evento.DonanteId,
                NombreDonante = evento.NombreDonante,
                ProgramaId = evento.ProgramaId,
                NombrePrograma = nombrePrograma,
                CodigoPeriodo = evento.CodigoPeriodo,
                MontoAportado = evento.Monto,
                Moneda = evento.Moneda
            },
            ct);
    }

    private async Task ProyectarPagoConfirmadoAsync(string payloadJson, CancellationToken ct)
    {
        var evento = JsonSerializer.Deserialize<PagoConfirmadoV1>(payloadJson, JsonOptions)
            ?? throw new InvalidOperationException("No se pudo deserializar PagoConfirmadoV1.");

        var yaExiste = await _db.ProyeccionDesembolsos
            .AsNoTracking()
            .AnyAsync(d => d.Id == evento.PagoId, ct);

        if (yaExiste)
        {
            return;
        }

        // Derivar periodo del calendario proyectado más reciente o del semestre calendario del pago.
        var periodoMasReciente = await _db.ProyeccionPeriodos
            .AsNoTracking()
            .OrderByDescending(p => p.CodigoPeriodo)
            .Select(p => p.CodigoPeriodo)
            .FirstOrDefaultAsync(ct);

        var semestre = evento.FechaPago.Month <= 6 ? 1 : 2;
        var codigoPeriodo = !string.IsNullOrWhiteSpace(periodoMasReciente)
            ? periodoMasReciente
            : $"{evento.FechaPago.Year}-{semestre}";

        await _db.ProyeccionDesembolsos.AddAsync(
            new DesembolsoImpactoProyeccion
            {
                Id = evento.PagoId,
                ProgramaId = evento.ProgramaId,
                CodigoPeriodo = codigoPeriodo,
                Monto = evento.Monto,
                Moneda = evento.Moneda,
                Estado = EstadoDesembolsoProyectado.CONFIRMADO
            },
            ct);
    }

    private async Task AsegurarPeriodoEnProyeccionAsync(string codigoPeriodo, CancellationToken ct)
    {
        var codigo = codigoPeriodo.Trim();
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return;
        }

        var existeEnDb = await _db.ProyeccionPeriodos
            .AsNoTracking()
            .AnyAsync(p => p.CodigoPeriodo == codigo, ct);

        var existeEnTracker = _db.ProyeccionPeriodos.Local
            .Any(p => string.Equals(p.CodigoPeriodo, codigo, StringComparison.OrdinalIgnoreCase));

        if (!existeEnDb && !existeEnTracker)
        {
            await _db.ProyeccionPeriodos.AddAsync(
                new PeriodoCalendarioProyeccion { CodigoPeriodo = codigo },
                ct);
        }
    }
}
