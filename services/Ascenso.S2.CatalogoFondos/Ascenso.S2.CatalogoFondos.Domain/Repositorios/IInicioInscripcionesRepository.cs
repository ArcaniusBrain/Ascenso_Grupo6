using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Repositorios;

public interface IInicioInscripcionesRepository
{
    Task<InicioInscripcionesPeriodo?> ObtenerPorUniversidadYPeriodoAsync(
        Guid universidadId,
        CodigoPeriodo codigoPeriodo,
        CancellationToken ct = default);

    Task<IReadOnlyList<InicioInscripcionesPeriodo>> ListarPorPeriodoAsync(
        CodigoPeriodo codigoPeriodo,
        CancellationToken ct = default);

    Task AgregarAsync(InicioInscripcionesPeriodo inicio, CancellationToken ct = default);
}
