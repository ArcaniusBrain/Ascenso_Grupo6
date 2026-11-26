using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Eventos;

public sealed record UniversidadRegistradaEvento(
    Guid UniversidadId,
    string Nombre,
    string ViaDatosAcademicos,
    DateTimeOffset Fecha) : IEventoDeDominio;

public sealed record CostoMatriculaRegistradoEvento(
    Guid UniversidadId,
    string CodigoPeriodo,
    decimal Monto,
    string Moneda,
    int VersionRegistro,
    DateTimeOffset Fecha) : IEventoDeDominio;
