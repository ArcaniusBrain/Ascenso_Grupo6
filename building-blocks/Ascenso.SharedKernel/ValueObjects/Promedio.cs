using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.ValueObjects;

public sealed record Promedio
{
    public decimal Valor { get; }

    public Promedio(decimal valor)
    {
        if (valor < 0m)
        {
            throw new ExcepcionDeDominio("El promedio académico no puede ser negativo.", "PROMEDIO_NEGATIVO");
        }

        Valor = decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
    }

    public bool EsMayorOIgual(Promedio otro)
    {
        ArgumentNullException.ThrowIfNull(otro);
        return Valor >= otro.Valor;
    }

    public override string ToString() => Valor.ToString("0.00");
}
