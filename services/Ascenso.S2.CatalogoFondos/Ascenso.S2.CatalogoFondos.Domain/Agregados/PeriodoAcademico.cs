using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Entidad interna de CalendarioAcademico que representa un periodo académico en la secuencia institucional.
/// No se elimina ni renombra para preservar la integridad histórica y trazabilidad (Anexo B · HU-38.1 #3, C15).
/// </summary>
public sealed class PeriodoAcademico : Entidad<Guid>
{
    public Guid CalendarioAcademicoId { get; private set; }
    public CodigoPeriodo Codigo { get; private set; } = default!;
    public EstadoPeriodo Estado { get; private set; }
    public bool EsActual { get; private set; }
    public DateTimeOffset RegistradoEnUtc { get; private set; }

    private PeriodoAcademico()
    {
    }

    internal PeriodoAcademico(
        Guid id,
        Guid calendarioAcademicoId,
        CodigoPeriodo codigo,
        DateTimeOffset registradoEnUtc) : base(id)
    {
        CalendarioAcademicoId = calendarioAcademicoId;
        Codigo = codigo;
        Estado = EstadoPeriodo.ABIERTO;
        EsActual = false;
        RegistradoEnUtc = registradoEnUtc;
    }

    internal void MarcarComoActual()
    {
        EsActual = true;
    }

    internal void DesmarcarActual()
    {
        EsActual = false;
    }

    internal void Cerrar()
    {
        Estado = EstadoPeriodo.CERRADO;
    }
}
