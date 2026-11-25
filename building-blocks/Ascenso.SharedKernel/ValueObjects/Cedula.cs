using System.Text.RegularExpressions;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.ValueObjects;

/// <summary>
/// Identificación única de una persona en ASCENSO (R1, DDD §5).
/// </summary>
public sealed partial record Cedula
{
    public string Numero { get; }

    public Cedula(string numero)
    {
        var limpia = (numero ?? string.Empty).Trim().ToUpperInvariant().Replace(".", string.Empty).Replace(" ", string.Empty);
        if (string.IsNullOrWhiteSpace(limpia) || !FormatoCedula().IsMatch(limpia))
        {
            throw new ExcepcionDeDominio(
                "La cédula debe tener un formato válido (ej. V-12345678, E-12345678 o solo dígitos de 6 a 12 caracteres).",
                "CEDULA_INVALIDA");
        }

        Numero = limpia;
    }

    [GeneratedRegex(@"^([VEJP]-)?\d{6,12}$", RegexOptions.Compiled)]
    private static partial Regex FormatoCedula();

    public override string ToString() => Numero;
}
