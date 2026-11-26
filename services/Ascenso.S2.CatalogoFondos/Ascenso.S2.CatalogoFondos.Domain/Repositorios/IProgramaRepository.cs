using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Repositorios;

public interface IProgramaRepository
{
    Task<Programa?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        Guid? excluirProgramaId = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Programa>> ListarTodosAsync(CancellationToken ct = default);

    Task<int> ContarPorPeriodoAsync(CodigoPeriodo codigoPeriodo, CancellationToken ct = default);

    Task AgregarAsync(Programa programa, CancellationToken ct = default);
}
