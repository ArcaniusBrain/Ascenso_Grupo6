using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.Trazabilidad;

/// <summary>
/// Referencia tipada al caso afectado por una decisión (DDD v3.3 §14).
/// </summary>
public sealed record SujetoRef
{
    public TipoSujeto Tipo { get; }
    public string Id { get; }

    public SujetoRef(TipoSujeto tipo, string id)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new ExcepcionDeDominio("El tipo de sujeto de la decisión no es válido.", "SUJETO_TIPO_INVALIDO");
        }

        var limpio = (id ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            throw new ExcepcionDeDominio("El identificador del sujeto de la decisión es obligatorio.", "SUJETO_ID_OBLIGATORIO");
        }

        Tipo = tipo;
        Id = limpio;
    }

    public static SujetoRef DesdeGuid(TipoSujeto tipo, Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador del sujeto de la decisión es obligatorio.", "SUJETO_ID_OBLIGATORIO");
        }

        return new SujetoRef(tipo, id.ToString("D"));
    }
}
