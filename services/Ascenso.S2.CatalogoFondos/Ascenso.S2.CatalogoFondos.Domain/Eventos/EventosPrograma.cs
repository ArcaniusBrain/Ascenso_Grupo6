using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Eventos;

public sealed record ProgramaCreadoEvento(
    Guid ProgramaId,
    string Nombre,
    string PeriodoInicio,
    int CupoBecarios,
    int DuracionPeriodos,
    decimal MontoPorPeriodo,
    string Moneda,
    DateTimeOffset Fecha) : IEventoDeDominio;
