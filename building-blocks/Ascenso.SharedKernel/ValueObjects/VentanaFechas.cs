using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.ValueObjects;

/// <summary>
/// Rango de fechas de negocio (R20, DDD §5, D-7).
/// Evalúa en UTC-4 incluyendo el día de apertura desde 00:00 y el día de cierre hasta las 23:59:59.999.
/// </summary>
public sealed record VentanaFechas
{
    public DateOnly Apertura { get; }
    public DateOnly Cierre { get; }

    public VentanaFechas(DateOnly apertura, DateOnly cierre)
    {
        if (cierre < apertura)
        {
            throw new ExcepcionDeDominio("La fecha de cierre no puede ser anterior a la fecha de apertura.", "VENTANA_INVALIDA");
        }

        Apertura = apertura;
        Cierre = cierre;
    }

    public bool Contiene(DateTimeOffset momento)
    {
        var enCaracas = RelojSistemaCaracas.ConvertirACaracas(momento);
        var inicio = RelojSistemaCaracas.InicioDeDiaCaracas(Apertura);
        var limiteExclusivo = RelojSistemaCaracas.InicioDeDiaCaracas(Cierre.AddDays(1));
        return enCaracas >= inicio && enCaracas < limiteExclusivo;
    }

    public bool EstaVencida(DateTimeOffset momento)
    {
        var enCaracas = RelojSistemaCaracas.ConvertirACaracas(momento);
        var limiteExclusivo = RelojSistemaCaracas.InicioDeDiaCaracas(Cierre.AddDays(1));
        return enCaracas >= limiteExclusivo;
    }
}
