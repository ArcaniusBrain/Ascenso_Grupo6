using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Eventos;

public sealed record CupoLiberadoEvento(
    Guid ProgramaId,
    Guid? CicloId,
    Guid ReservaId,
    string Motivo,
    decimal MontoLiberado,
    string Moneda,
    DateTimeOffset Fecha) : IEventoDeDominio;
