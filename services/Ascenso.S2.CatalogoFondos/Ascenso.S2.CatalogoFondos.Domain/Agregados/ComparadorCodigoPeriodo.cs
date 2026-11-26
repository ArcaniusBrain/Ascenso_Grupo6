using System.Globalization;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Comparador cronológico ascendente para códigos de periodo académico (Anexo B · HU-38.1 #1, C15).
/// Ordena numéricamente segmentos separados por '-' o '/' (ej. 2025-2 antes que 2026-1 y 2026-2 antes que 2026-10).
/// </summary>
public sealed class ComparadorCodigoPeriodo : IComparer<CodigoPeriodo>
{
    public static readonly ComparadorCodigoPeriodo Instancia = new();

    private ComparadorCodigoPeriodo()
    {
    }

    public int Compare(CodigoPeriodo? x, CodigoPeriodo? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        var partesX = x.Valor.Split(['-', '/'], StringSplitOptions.RemoveEmptyEntries);
        var partesY = y.Valor.Split(['-', '/'], StringSplitOptions.RemoveEmptyEntries);

        var longitudMin = Math.Min(partesX.Length, partesY.Length);
        for (var i = 0; i < longitudMin; i++)
        {
            var esNumX = int.TryParse(partesX[i], NumberStyles.None, CultureInfo.InvariantCulture, out var numX);
            var esNumY = int.TryParse(partesY[i], NumberStyles.None, CultureInfo.InvariantCulture, out var numY);

            var cmp = (esNumX, esNumY) switch
            {
                (true, true) => numX.CompareTo(numY),
                _ => string.Compare(partesX[i], partesY[i], StringComparison.OrdinalIgnoreCase)
            };

            if (cmp != 0) return cmp;
        }

        if (partesX.Length != partesY.Length)
        {
            return partesX.Length.CompareTo(partesY.Length);
        }

        return string.Compare(x.Valor, y.Valor, StringComparison.OrdinalIgnoreCase);
    }
}
