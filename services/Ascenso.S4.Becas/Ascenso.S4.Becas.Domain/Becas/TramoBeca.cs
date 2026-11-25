using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S4.Becas.Domain.Becas;

/// <summary>
/// Tramo aprobado por periodo académico dentro de una beca (DDD_v3.3 §11b).
/// </summary>
public sealed class TramoBeca : Entidad<Guid>
{
    public CodigoPeriodo Periodo { get; private set; } = null!;
    public Dinero MontoAprobado { get; private set; } = null!;
    public OrigenTramo Origen { get; private set; }

    private TramoBeca()
    {
    }

    public TramoBeca(Guid id, CodigoPeriodo periodo, Dinero montoAprobado, OrigenTramo origen)
        : base(id)
    {
        if (id == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador del tramo de beca es obligatorio.", "TRAMO_ID_INVALIDO");
        }

        ArgumentNullException.ThrowIfNull(periodo);
        ArgumentNullException.ThrowIfNull(montoAprobado);

        Periodo = periodo;
        MontoAprobado = montoAprobado;
        Origen = origen;
    }
}
