using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Application.Puertos;

public sealed record EstadoVinculoReservaS4(
    Guid ReservaId,
    bool ExisteVinculo,
    string TipoVinculo,
    Guid? RecursoId,
    DateTimeOffset ConsultadoEnUtc);

public sealed record ResultadoConsultaVinculoS4(
    bool Disponible,
    EstadoVinculoReservaS4? Estado,
    string? Error = null);

/// <summary>
/// Puerto saliente S2 -> S4 para la conciliación segura de reservas sin vincular (ADR-26).
/// Invoca GET /internal/v1/reservas/{reservaId}/vinculo con scope ofertas.consultar-vinculo.
/// </summary>
public interface IConsultorVinculoReservaS4
{
    Task<ResultadoConsultaVinculoS4> ConsultarVinculoAsync(Guid reservaId, CancellationToken ct = default);
}

/// <summary>
/// Ejecuta operaciones sobre PresupuestoPrograma con reintento controlado ante conflictos de concurrencia optimista (ADR-25),
/// manteniendo S2.Application libre de dependencias a EF Core.
/// </summary>
public interface IEjecutorConcurrenciaPresupuesto
{
    Task<TResultado> EjecutarConReintentoAsync<TResultado>(
        Func<CancellationToken, Task<TResultado>> accion,
        CancellationToken ct = default);
}

/// <summary>
/// Sincroniza idempotentemente PresupuestoPrograma dentro de S2 al crear programas (RF-11) y registrar aportes (RF-13).
/// </summary>
public interface ISincronizadorPresupuestoPrograma
{
    Task InicializarParaProgramaAsync(Programa programa, DateTimeOffset ahoraUtc, CancellationToken ct = default);

    Task AplicarAporteIdempotenteAsync(
        Guid aporteId,
        Guid programaId,
        Dinero monto,
        DateTimeOffset ahoraUtc,
        CancellationToken ct = default);
}
