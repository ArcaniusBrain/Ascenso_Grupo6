using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Application.Puertos;

/// <summary>
/// Puerto de Application para registrar eventos de integración en el Outbox dentro de la transacción actual (ADR-12).
/// </summary>
public interface IEscritorOutboxCatalogo
{
    Task RegistrarEventoAsync<TEvento>(
        Guid eventId,
        string eventType,
        int schemaVersion,
        TEvento payload,
        Guid correlationId,
        Guid causationId,
        DateTimeOffset occurredAtUtc,
        CancellationToken ct = default);
}

/// <summary>
/// Puerto de Application para garantizar idempotencia por operationId en comandos mutadores (ADR-13).
/// </summary>
public interface IRegistroIdempotenciaComandos
{
    Task<OperacionConfirmadaDto?> BuscarAsync(Guid operationId, CancellationToken ct = default);
    Task RegistrarAsync(Guid operationId, OperacionConfirmadaDto resultado, DateTimeOffset procesadoEnUtc, CancellationToken ct = default);
}

/// <summary>
/// Puerto de lectura para consultar cuántos elementos (programas, ciclos, renovaciones) están ligados a un periodo (HU-38.1 E5).
/// </summary>
public interface IConsultorElementosPeriodo
{
    Task<int> ContarElementosLigadosAsync(CodigoPeriodo codigoPeriodo, CancellationToken ct = default);
}
