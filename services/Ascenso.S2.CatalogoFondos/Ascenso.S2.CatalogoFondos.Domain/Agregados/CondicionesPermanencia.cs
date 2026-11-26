using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Value Object estructurado de condiciones de permanencia, pausa máxima y renovación (DDD v3.3 §8, HU-11.2, RF-27).
/// </summary>
public sealed record CondicionesPermanencia
{
    public const string MensajePromedioInvalido = "El promedio mínimo debe ser un número.";
    public const string MensajePromedioNegativo = "El promedio mínimo no puede ser negativo.";
    public const string MensajeHorasNegativas = "Las horas de servicio no pueden ser negativas.";
    public const string MensajePausaNegativa = "La duración máxima de pausa no puede ser negativa.";
    public const string LeyendaSinHorasServicio = "Horas de servicio: no aplica";

    public decimal PromedioMinimo { get; private set; }
    public bool ExigeHorasServicio { get; private set; }
    public int? HorasServicio { get; private set; }
    public string ReportesExigidosTexto { get; private set; } = string.Empty;
    public int DuracionMaxPausaMeses { get; private set; }
    public string DescripcionRenovacion { get; private set; } = string.Empty;

    private CondicionesPermanencia()
    {
    }

    private CondicionesPermanencia(
        decimal promedioMinimo,
        bool exigeHorasServicio,
        int? horasServicio,
        string reportesExigidosTexto,
        int duracionMaxPausaMeses,
        string descripcionRenovacion)
    {
        PromedioMinimo = promedioMinimo;
        ExigeHorasServicio = exigeHorasServicio;
        HorasServicio = horasServicio;
        ReportesExigidosTexto = reportesExigidosTexto;
        DuracionMaxPausaMeses = duracionMaxPausaMeses;
        DescripcionRenovacion = descripcionRenovacion;
    }

    public static CondicionesPermanencia Crear(
        decimal? promedioMinimo,
        bool exigeHorasServicio,
        int? horasServicio,
        IEnumerable<string>? reportesExigidos,
        int? duracionMaxPausaMeses,
        string? descripcionRenovacion)
    {
        if (promedioMinimo is null)
        {
            throw new ExcepcionDeDominio(MensajePromedioInvalido, "PROMEDIO_MINIMO_INVALIDO");
        }

        if (promedioMinimo.Value < 0m)
        {
            throw new ExcepcionDeDominio(MensajePromedioNegativo, "PROMEDIO_MINIMO_NEGATIVO");
        }

        int? horasValidadas = null;
        if (exigeHorasServicio)
        {
            if (horasServicio is null || horasServicio.Value < 0)
            {
                throw new ExcepcionDeDominio(MensajeHorasNegativas, "HORAS_SERVICIO_NEGATIVAS");
            }

            horasValidadas = horasServicio.Value;
        }

        var pausaMeses = duracionMaxPausaMeses ?? 0;
        if (pausaMeses < 0)
        {
            throw new ExcepcionDeDominio(MensajePausaNegativa, "DURACION_PAUSA_NEGATIVA");
        }

        var reportesLimpios = (reportesExigidos ?? [])
            .Select(r => (r ?? string.Empty).Trim())
            .Where(r => r.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new CondicionesPermanencia(
            decimal.Round(promedioMinimo.Value, 2, MidpointRounding.AwayFromZero),
            exigeHorasServicio,
            horasValidadas,
            string.Join("||", reportesLimpios),
            pausaMeses,
            (descripcionRenovacion ?? string.Empty).Trim());
    }

    public IReadOnlyList<string> ObtenerReportesExigidos() =>
        string.IsNullOrWhiteSpace(ReportesExigidosTexto)
            ? []
            : ReportesExigidosTexto.Split("||", StringSplitOptions.RemoveEmptyEntries);

    public string ObtenerLeyendaHorasServicio() =>
        ExigeHorasServicio ? $"{HorasServicio} horas" : LeyendaSinHorasServicio;
}
