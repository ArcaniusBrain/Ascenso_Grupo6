using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Eventos;

public sealed record PeriodoAbiertoEvento(
    Guid CalendarioId,
    string CodigoPeriodo,
    DateTimeOffset Fecha) : IEventoDeDominio;

public sealed record PeriodoMarcadoActualEvento(
    Guid CalendarioId,
    string CodigoPeriodo,
    string? CodigoPeriodoAnterior,
    DateTimeOffset Fecha) : IEventoDeDominio;
