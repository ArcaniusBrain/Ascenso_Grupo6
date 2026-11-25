using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.ValueObjects;

/// <summary>
/// Value Object Dinero (DDD §5, lámina 25). Inmutable: cantidad no negativa y moneda ISO de 3 letras.
/// </summary>
public sealed record Dinero
{
    public decimal Cantidad { get; }
    public string Moneda { get; }

    public Dinero(decimal cantidad, string moneda)
    {
        if (cantidad < 0m)
        {
            throw new ExcepcionDeDominio("La cantidad de dinero no puede ser negativa.", "DINERO_NEGATIVO");
        }

        var normalizada = (moneda ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizada.Length != 3 || !normalizada.All(char.IsAsciiLetterUpper))
        {
            throw new ExcepcionDeDominio("La moneda debe tener exactamente 3 letras ISO (por ejemplo USD).", "MONEDA_INVALIDA");
        }

        Cantidad = decimal.Round(cantidad, 2, MidpointRounding.AwayFromZero);
        Moneda = normalizada;
    }

    public static Dinero Cero(string moneda = "USD") => new(0m, moneda);

    public Dinero Sumar(Dinero otro)
    {
        ArgumentNullException.ThrowIfNull(otro);
        ValidarMismaMoneda(otro);
        return new Dinero(Cantidad + otro.Cantidad, Moneda);
    }

    public Dinero Restar(Dinero otro)
    {
        ArgumentNullException.ThrowIfNull(otro);
        ValidarMismaMoneda(otro);

        if (otro.Cantidad > Cantidad)
        {
            throw new ExcepcionDeDominio("La resta de dinero no puede producir un saldo negativo.", "DINERO_INSUFICIENTE");
        }

        return new Dinero(Cantidad - otro.Cantidad, Moneda);
    }

    public Dinero Multiplicar(int factor)
    {
        if (factor < 0)
        {
            throw new ExcepcionDeDominio("El factor multiplicador no puede ser negativo.", "FACTOR_NEGATIVO");
        }

        return new Dinero(Cantidad * factor, Moneda);
    }

    public bool EsMayorQue(Dinero otro)
    {
        ArgumentNullException.ThrowIfNull(otro);
        ValidarMismaMoneda(otro);
        return Cantidad > otro.Cantidad;
    }

    public bool EsMayorOIgualQue(Dinero otro)
    {
        ArgumentNullException.ThrowIfNull(otro);
        ValidarMismaMoneda(otro);
        return Cantidad >= otro.Cantidad;
    }

    private void ValidarMismaMoneda(Dinero otro)
    {
        if (!string.Equals(Moneda, otro.Moneda, StringComparison.Ordinal))
        {
            throw new ExcepcionDeDominio(
                $"No se pueden operar monedas distintas: {Moneda} y {otro.Moneda}.",
                "MONEDA_DISTINTA");
        }
    }

    public override string ToString() => $"{Moneda} {Cantidad:0.00}";
}
