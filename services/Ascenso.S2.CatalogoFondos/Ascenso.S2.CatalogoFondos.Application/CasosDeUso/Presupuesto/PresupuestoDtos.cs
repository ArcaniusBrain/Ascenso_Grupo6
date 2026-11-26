namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Presupuesto;

public sealed record PresupuestoProgramaDto(
    Guid ProgramaId,
    string NombrePrograma,
    string Moneda,
    decimal PresupuestoTotal,
    decimal TotalOtorgado,
    decimal ComprometidoOfertasPendientes,
    decimal ComprometidoRenovacionesPendientes,
    decimal PresupuestoDisponible,
    decimal Desembolsado,
    int CupoTotal,
    int CupoOcupado,
    int CupoDisponible,
    string? MensajeSinAportes,
    int Version);

/// <summary>
/// Contrato de respuesta de frontera privada (contracts/openapi-interno.yaml: ReservaInternaResponse).
/// </summary>
public sealed record ReservaInternaResponseDto(
    Guid ReservaId,
    string Estado,
    decimal MontoCalculado,
    int DuracionPeriodos,
    string Moneda);

/// <summary>
/// Contrato de respuesta de frontera privada (contracts/openapi-interno.yaml: DisponibilidadInterna).
/// </summary>
public sealed record DisponibilidadInternaDto(
    decimal Disponible,
    int CupoDisponible);

public sealed record ResultadoConciliacionReservasDto(
    int Evaluadas,
    int Vinculadas,
    int Liberadas,
    int OmitidasPorFalloS4);
