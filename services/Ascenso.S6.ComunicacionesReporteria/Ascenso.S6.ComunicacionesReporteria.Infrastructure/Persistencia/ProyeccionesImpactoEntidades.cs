using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure.Persistencia;

/// <summary>
/// Tablas de proyección CQRS locales en s6_comunicaciones alimentadas por eventos de S2, S4 y S5 (ADR-05, ADR-07).
/// </summary>
public sealed class PeriodoCalendarioProyeccion
{
    public string CodigoPeriodo { get; set; } = string.Empty;
}

public sealed class AporteImpactoProyeccion
{
    public Guid Id { get; set; }
    public Guid DonanteId { get; set; }
    public string NombreDonante { get; set; } = string.Empty;
    public Guid ProgramaId { get; set; }
    public string NombrePrograma { get; set; } = string.Empty;
    public string CodigoPeriodo { get; set; } = string.Empty;
    public decimal MontoAportado { get; set; }
    public string Moneda { get; set; } = "USD";
}

public sealed class BecarioImpactoProyeccion
{
    public Guid Id { get; set; }
    public Guid BecarioId { get; set; }
    public Guid ProgramaId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Universidad { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public decimal? IngresoFamiliarSensible { get; set; }
    public string? ComposicionFamiliarSensible { get; set; }
}

public sealed class DesembolsoImpactoProyeccion
{
    public Guid Id { get; set; }
    public Guid ProgramaId { get; set; }
    public string CodigoPeriodo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "USD";
    public EstadoDesembolsoProyectado Estado { get; set; }
}

public sealed class ReporteAcademicoImpactoProyeccion
{
    public Guid Id { get; set; }
    public Guid BecarioId { get; set; }
    public Guid ProgramaId { get; set; }
    public string CodigoPeriodo { get; set; } = string.Empty;
    public decimal Promedio { get; set; }
    public int MateriasAprobadas { get; set; }
    public int MateriasReprobadas { get; set; }
    public OrigenReporteAcademicoProyectado Origen { get; set; }
}
