using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Entidad interna append-only de aporte financiero de un donante a un programa y periodo (HU-13.2 Anexo A, DDD v3.3 §8).
/// Nunca se sobrescribe ni elimina para conservar historial legal y contable completo.
/// </summary>
public sealed class Aporte : Entidad<Guid>
{
    public Guid DonanteId { get; private set; }
    public Guid ProgramaId { get; private set; }
    public CodigoPeriodo Periodo { get; private set; } = default!;
    public Dinero Monto { get; private set; } = default!;
    public DateTimeOffset RegistradoEnUtc { get; private set; }

    private Aporte()
    {
    }

    internal Aporte(
        Guid id,
        Guid donanteId,
        Guid programaId,
        CodigoPeriodo periodo,
        Dinero monto,
        DateTimeOffset registradoEnUtc) : base(id)
    {
        DonanteId = donanteId;
        ProgramaId = programaId;
        Periodo = periodo;
        Monto = monto;
        RegistradoEnUtc = registradoEnUtc;
    }
}
