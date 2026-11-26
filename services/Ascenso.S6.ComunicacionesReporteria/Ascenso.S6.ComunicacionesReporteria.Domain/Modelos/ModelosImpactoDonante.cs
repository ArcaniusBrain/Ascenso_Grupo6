namespace Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;

public enum EstadoDesembolsoProyectado
{
    PROGRAMADO = 1,
    CONFIRMADO = 2,
    FALLIDO = 3,
    DEVUELTO = 4
}

public enum OrigenReporteAcademicoProyectado
{
    API = 1,
    MANUAL = 2
}

/// <summary>
/// Entrada interna de proyección CQRS de aportes del donante (nunca expuesta directamente al cliente).
/// </summary>
public sealed record AporteDonanteProyectado(
    Guid DonanteId,
    string NombreDonante,
    Guid ProgramaId,
    string NombrePrograma,
    string CodigoPeriodo,
    decimal MontoAportado,
    string Moneda);

/// <summary>
/// Entrada interna de proyección CQRS de becarios del programa.
/// Puede contener campos sensibles de origen (IngresoFamiliar, ComposicionFamiliar) para comprobar en pruebas
/// que la capa única de anonimización JAMÁS los copia a la salida.
/// </summary>
public sealed record BecarioProyectadoEntrada(
    Guid BecarioId,
    Guid ProgramaId,
    string NombreCompleto,
    string Cedula,
    string Universidad,
    string Carrera,
    decimal? IngresoFamiliarSensible = null,
    string? ComposicionFamiliarSensible = null);

public sealed record DesembolsoProyectadoEntrada(
    Guid ProgramaId,
    string CodigoPeriodo,
    decimal Monto,
    string Moneda,
    EstadoDesembolsoProyectado Estado);

public sealed record ReporteAcademicoProyectadoEntrada(
    Guid BecarioId,
    Guid ProgramaId,
    string CodigoPeriodo,
    decimal Promedio,
    int MateriasAprobadas,
    int MateriasReprobadas,
    OrigenReporteAcademicoProyectado Origen);

// ── Modelos de salida estrictamente anonimizados (imposibilidad estructural de fuga socioeconómica) ──

public sealed record DistribucionAgregadaItem(string Etiqueta, int Cantidad);

public sealed record PerfilGeneralAgregado(
    int TotalBecarios,
    IReadOnlyList<DistribucionAgregadaItem> PorUniversidad,
    IReadOnlyList<DistribucionAgregadaItem> PorCarrera);

public sealed record EjecucionPresupuestariaImpacto(
    decimal AporteDonanteEnPeriodo,
    decimal DesembolsadoConfirmado,
    string Moneda);

public sealed record ResultadosAcademicosAgregados(
    int CantidadReportes,
    decimal? PromedioGeneral,
    int TotalMateriasAprobadas,
    int TotalMateriasReprobadas,
    string? MensajeSinDatos);

/// <summary>
/// Datos no socioeconómicos visibles SOLO para becarios con ConsentimientoDonante vigente para este donante (HU-33.2, C20).
/// Incluye nombre, cédula, universidad, carrera, programa y rendimiento académico general; NUNCA ingreso ni composición familiar.
/// </summary>
public sealed record BecarioPresentadoImpacto(
    string NombreCompleto,
    string Cedula,
    string Universidad,
    string Carrera,
    string Programa,
    decimal? PromedioAcademico);

public sealed record ProgramaImpactoReporte(
    Guid ProgramaId,
    string NombrePrograma,
    int NumeroBecarios,
    PerfilGeneralAgregado PerfilGeneral,
    EjecucionPresupuestariaImpacto EjecucionPresupuestaria,
    ResultadosAcademicosAgregados ResultadosAcademicos,
    IReadOnlyList<BecarioPresentadoImpacto> BecariosPresentados);

public sealed record ReporteImpactoContenido(
    Guid DonanteId,
    string NombreDonante,
    string CodigoPeriodo,
    DateTimeOffset GeneradoEnUtc,
    int TotalBecariosUnicos,
    decimal TotalAportadoDonante,
    decimal TotalDesembolsadoConfirmado,
    string Moneda,
    IReadOnlyList<ProgramaImpactoReporte> Programas);
