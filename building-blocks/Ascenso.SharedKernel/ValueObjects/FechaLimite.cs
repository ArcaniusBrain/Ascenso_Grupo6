using Ascenso.SharedKernel.Tiempo;

namespace Ascenso.SharedKernel.ValueObjects;

/// <summary>
/// Fecha límite de negocio (R20, DDD §5, D-7).
/// El día indicado se incluye completo hasta las 23:59:59.999 en UTC-4 (America/Caracas).
/// </summary>
public sealed record FechaLimite
{
    public DateOnly Fecha { get; }

    public FechaLimite(DateOnly fecha)
    {
        Fecha = fecha;
    }

    public bool Vencida(DateTimeOffset momento)
    {
        var enCaracas = RelojSistemaCaracas.ConvertirACaracas(momento);
        var limiteExclusivo = RelojSistemaCaracas.InicioDeDiaCaracas(Fecha.AddDays(1));
        return enCaracas >= limiteExclusivo;
    }
}
