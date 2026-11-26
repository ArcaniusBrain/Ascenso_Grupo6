using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Entidad interna append-only de UniversidadAliada para el costo de matrícula por periodo (HU-14.2, Anexo A).
/// Cada corrección inserta una nueva fila con VersionRegistro incremental sin sobrescribir registros anteriores.
/// </summary>
public sealed class CostoMatricula : Entidad<Guid>
{
    public Guid UniversidadAliadaId { get; private set; }
    public CodigoPeriodo Periodo { get; private set; } = default!;
    public Dinero Costo { get; private set; } = default!;
    public int VersionRegistro { get; private set; }
    public DateTimeOffset RegistradoEnUtc { get; private set; }

    private CostoMatricula()
    {
    }

    internal CostoMatricula(
        Guid id,
        Guid universidadAliadaId,
        CodigoPeriodo periodo,
        Dinero costo,
        int versionRegistro,
        DateTimeOffset registradoEnUtc) : base(id)
    {
        UniversidadAliadaId = universidadAliadaId;
        Periodo = periodo;
        Costo = costo;
        VersionRegistro = versionRegistro;
        RegistradoEnUtc = registradoEnUtc;
    }
}
