using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.ValueObjects;

public sealed record Justificacion
{
    public string Texto { get; }

    public Justificacion(string texto)
    {
        var limpio = (texto ?? string.Empty).Trim();
        if (limpio.Length < 3)
        {
            throw new ExcepcionDeDominio("La justificación es obligatoria y debe tener al menos 3 caracteres.", "JUSTIFICACION_INVALIDA");
        }

        Texto = limpio;
    }

    public override string ToString() => Texto;
}
