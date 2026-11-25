using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.ValueObjects;

/// <summary>
/// Identificador de periodo académico usado fuera de CalendarioAcademico (ej. "2026-2", DDD §5, D-5).
/// </summary>
public sealed record CodigoPeriodo
{
    public string Valor { get; }

    public CodigoPeriodo(string valor)
    {
        var limpio = (valor ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(limpio) || limpio.Length > 20)
        {
            throw new ExcepcionDeDominio("El código de periodo académico es obligatorio (máximo 20 caracteres).", "CODIGO_PERIODO_INVALIDO");
        }

        Valor = limpio;
    }

    public override string ToString() => Valor;
}
