namespace Ascenso.S2.CatalogoFondos.Domain.Repositorios;

public interface IUnidadTrabajoCatalogo
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
