using Ascenso.S2.CatalogoFondos.Domain.Agregados;

namespace Ascenso.S2.CatalogoFondos.Domain.Repositorios;

public interface IPresupuestoProgramaRepository
{
    Task<PresupuestoPrograma?> ObtenerPorProgramaIdAsync(Guid programaId, CancellationToken ct = default);

    Task<PresupuestoPrograma?> ObtenerPorReservaIdAsync(Guid reservaId, CancellationToken ct = default);

    Task<PresupuestoPrograma?> ObtenerPorOperationIdAsync(Guid operationId, CancellationToken ct = default);

    Task<IReadOnlyList<ReservaPresupuesto>> ListarReservasSinVinculoAnterioresAAsync(
        DateTimeOffset limiteUtc,
        CancellationToken ct = default);

    Task AgregarAsync(PresupuestoPrograma presupuesto, CancellationToken ct = default);
}
