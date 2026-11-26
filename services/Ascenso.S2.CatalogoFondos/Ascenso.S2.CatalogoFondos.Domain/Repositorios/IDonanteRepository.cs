using Ascenso.S2.CatalogoFondos.Domain.Agregados;

namespace Ascenso.S2.CatalogoFondos.Domain.Repositorios;

public interface IDonanteRepository
{
    Task<Donante?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        Guid? excluirDonanteId = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Donante>> ListarTodosAsync(CancellationToken ct = default);

    Task AgregarAsync(Donante donante, CancellationToken ct = default);
}
