using Ascenso.S2.CatalogoFondos.Domain.Agregados;

namespace Ascenso.S2.CatalogoFondos.Domain.Repositorios;

public interface ICalendarioAcademicoRepository
{
    Task<CalendarioAcademico?> ObtenerInstitucionalAsync(CancellationToken ct = default);
    Task AgregarAsync(CalendarioAcademico calendario, CancellationToken ct = default);
}
