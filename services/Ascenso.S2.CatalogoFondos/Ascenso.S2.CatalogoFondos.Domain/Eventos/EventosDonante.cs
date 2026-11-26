using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Eventos;

public sealed record DonanteRegistradoEvento(
    Guid DonanteId,
    string Nombre,
    string TipoDonante,
    DateTimeOffset Fecha) : IEventoDeDominio;

public sealed record AporteRegistradoEvento(
    Guid AporteId,
    Guid DonanteId,
    string NombreDonante,
    Guid ProgramaId,
    string CodigoPeriodo,
    decimal Monto,
    string Moneda,
    DateTimeOffset Fecha) : IEventoDeDominio;
