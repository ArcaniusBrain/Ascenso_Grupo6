using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Donantes;

public sealed record ContactoDonanteDto(
    string? Correo,
    string? Telefono);

public sealed record DonanteResumenDto(
    Guid Id,
    string Nombre,
    string Tipo,
    ContactoDonanteDto Contacto,
    int CantidadAportes,
    decimal TotalAportado,
    int Version);

public sealed record AporteDto(
    Guid Id,
    Guid DonanteId,
    string NombreDonante,
    Guid ProgramaId,
    string NombrePrograma,
    string CodigoPeriodo,
    decimal Monto,
    string Moneda,
    DateTimeOffset RegistradoEnUtc);

public sealed record HistorialAportesDonanteDto(
    Guid DonanteId,
    string NombreDonante,
    string TipoDonante,
    string? CodigoPeriodoFiltro,
    decimal TotalAportado,
    string? MensajeSinAportes,
    IReadOnlyList<AporteDto> Aportes);

public sealed record AporteRegistradoResultadoDto(
    AporteDto Aporte,
    OperacionConfirmadaDto Confirmacion);
