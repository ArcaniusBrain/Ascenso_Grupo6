using Ascenso.S2.CatalogoFondos.Domain.Agregados;

namespace Ascenso.S2.CatalogoFondos.Domain.Repositorios;

public interface IUniversidadAliadaRepository
{
    Task<UniversidadAliada?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteNombreNormalizadoAsync(
        string nombreNormalizado,
        Guid? excluirUniversidadId = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<UniversidadAliada>> ListarTodasAsync(CancellationToken ct = default);

    Task AgregarAsync(UniversidadAliada universidad, CancellationToken ct = default);
}
