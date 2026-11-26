using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Raíz de agregado Programa de becas (RF-11 · HU-11.1 y HU-11.2, DDD v3.3 §8, ADR-25).
/// Fija monto fijo por periodo, modalidad de pago, etiqueta COMPLETO/PARCIAL, donantes asociados,
/// restricciones estructuradas de elegibilidad y condiciones de permanencia, pausa y renovación.
/// </summary>
public sealed class Programa : RaizDeAgregado<Guid>
{
    public const string MensajeNombreObligatorio = "Indica el nombre del programa.";
    public const string MensajeCupoObligatorio = "Indica el cupo de becarios.";
    public const string MensajeCupoDebeSerPositivo = "El cupo de becarios debe ser mayor que cero.";
    public const string MensajeDuracionObligatoria = "Indica la duración del programa.";
    public const string MensajeDuracionDebeSerPositiva = "La duración del programa debe ser mayor que cero.";
    public const string MensajeMontoObligatorio = "Indica el monto de la beca por periodo.";
    public const string MensajeMontoDebeSerPositivo = "El monto de la beca por periodo debe ser mayor que cero.";
    public const string MensajeAlcanceObligatorio = "Elige si el programa es completo o parcial.";
    public const string MensajeModalidadObligatoria = "Elige la modalidad de pago del programa.";
    public const string MensajeDonanteDuplicado = "Este donante ya está asociado al programa.";
    public const string MensajeSinRestriccion = "Sin restricción de universidad ni carrera";

    private readonly List<AsociacionDonantePrograma> _donantes = [];
    private readonly List<RestriccionElegibilidad> _restricciones = [];

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public CodigoPeriodo PeriodoInicio { get; private set; } = default!;
    public int CupoBecarios { get; private set; }
    public int DuracionPeriodos { get; private set; }
    public Dinero MontoPorPeriodo { get; private set; } = default!;
    public ModalidadPago ModalidadPago { get; private set; }
    public AlcancePrograma Alcance { get; private set; }
    public bool PermiteAcumulacion { get; private set; }
    public CondicionesPermanencia? Condiciones { get; private set; }
    public DateTimeOffset CreadoEnUtc { get; private set; }

    public IReadOnlyCollection<AsociacionDonantePrograma> Donantes => _donantes.AsReadOnly();
    public IReadOnlyCollection<RestriccionElegibilidad> Restricciones => _restricciones.AsReadOnly();

    private Programa()
    {
    }

    private Programa(
        Guid id,
        string nombre,
        string nombreNormalizado,
        CodigoPeriodo periodoInicio,
        int cupoBecarios,
        int duracionPeriodos,
        Dinero montoPorPeriodo,
        ModalidadPago modalidadPago,
        AlcancePrograma alcance,
        bool permiteAcumulacion,
        DateTimeOffset creadoEnUtc) : base(id)
    {
        Nombre = nombre;
        NombreNormalizado = nombreNormalizado;
        PeriodoInicio = periodoInicio;
        CupoBecarios = cupoBecarios;
        DuracionPeriodos = duracionPeriodos;
        MontoPorPeriodo = montoPorPeriodo;
        ModalidadPago = modalidadPago;
        Alcance = alcance;
        PermiteAcumulacion = permiteAcumulacion;
        CreadoEnUtc = creadoEnUtc;
    }

    public static Programa Crear(
        string? nombre,
        CodigoPeriodo periodoInicio,
        int? cupoBecarios,
        int? duracionPeriodos,
        decimal? montoPorPeriodo,
        string? moneda,
        ModalidadPago? modalidadPago,
        AlcancePrograma? alcance,
        bool permiteAcumulacion,
        DateTimeOffset ahoraUtc)
    {
        var nombreLimpio = UniversidadAliada.LimpiarNombreVisible(nombre);
        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            throw new ExcepcionDeDominio(MensajeNombreObligatorio, "NOMBRE_PROGRAMA_OBLIGATORIO");
        }

        if (cupoBecarios is null)
        {
            throw new ExcepcionDeDominio(MensajeCupoObligatorio, "CUPO_BECARIOS_OBLIGATORIO");
        }

        if (cupoBecarios.Value <= 0)
        {
            throw new ExcepcionDeDominio(MensajeCupoDebeSerPositivo, "CUPO_BECARIOS_INVALIDO");
        }

        if (duracionPeriodos is null)
        {
            throw new ExcepcionDeDominio(MensajeDuracionObligatoria, "DURACION_PROGRAMA_OBLIGATORIA");
        }

        if (duracionPeriodos.Value <= 0)
        {
            throw new ExcepcionDeDominio(MensajeDuracionDebeSerPositiva, "DURACION_PROGRAMA_INVALIDA");
        }

        if (montoPorPeriodo is null)
        {
            throw new ExcepcionDeDominio(MensajeMontoObligatorio, "MONTO_BECA_OBLIGATORIO");
        }

        if (montoPorPeriodo.Value <= 0m)
        {
            throw new ExcepcionDeDominio(MensajeMontoDebeSerPositivo, "MONTO_BECA_INVALIDO");
        }

        if (modalidadPago is null || !Enum.IsDefined(modalidadPago.Value))
        {
            throw new ExcepcionDeDominio(MensajeModalidadObligatoria, "MODALIDAD_PAGO_OBLIGATORIA");
        }

        if (alcance is null || !Enum.IsDefined(alcance.Value))
        {
            throw new ExcepcionDeDominio(MensajeAlcanceObligatorio, "ALCANCE_PROGRAMA_OBLIGATORIO");
        }

        var dinero = new Dinero(montoPorPeriodo.Value, string.IsNullOrWhiteSpace(moneda) ? "USD" : moneda);
        // C17 / HU-01.2: un programa COMPLETO nunca permite acumulación.
        var acumulacionEfectiva = alcance.Value == AlcancePrograma.PARCIAL && permiteAcumulacion;

        var programa = new Programa(
            Guid.NewGuid(),
            nombreLimpio,
            UniversidadAliada.NormalizarClaveTexto(nombreLimpio),
            periodoInicio,
            cupoBecarios.Value,
            duracionPeriodos.Value,
            dinero,
            modalidadPago.Value,
            alcance.Value,
            acumulacionEfectiva,
            ahoraUtc);

        programa.Registrar(new ProgramaCreadoEvento(
            programa.Id,
            programa.Nombre,
            programa.PeriodoInicio.Valor,
            programa.CupoBecarios,
            programa.DuracionPeriodos,
            programa.MontoPorPeriodo.Cantidad,
            programa.MontoPorPeriodo.Moneda,
            ahoraUtc));

        return programa;
    }

    public void ActualizarConfiguracionBase(
        string? nuevoNombre,
        decimal? montoPorPeriodo,
        string? moneda,
        ModalidadPago? modalidadPago,
        AlcancePrograma? alcance,
        bool permiteAcumulacion)
    {
        var nombreLimpio = UniversidadAliada.LimpiarNombreVisible(nuevoNombre);
        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            throw new ExcepcionDeDominio(MensajeNombreObligatorio, "NOMBRE_PROGRAMA_OBLIGATORIO");
        }

        if (montoPorPeriodo is null)
        {
            throw new ExcepcionDeDominio(MensajeMontoObligatorio, "MONTO_BECA_OBLIGATORIO");
        }

        if (montoPorPeriodo.Value <= 0m)
        {
            throw new ExcepcionDeDominio(MensajeMontoDebeSerPositivo, "MONTO_BECA_INVALIDO");
        }

        if (modalidadPago is null || !Enum.IsDefined(modalidadPago.Value))
        {
            throw new ExcepcionDeDominio(MensajeModalidadObligatoria, "MODALIDAD_PAGO_OBLIGATORIA");
        }

        if (alcance is null || !Enum.IsDefined(alcance.Value))
        {
            throw new ExcepcionDeDominio(MensajeAlcanceObligatorio, "ALCANCE_PROGRAMA_OBLIGATORIO");
        }

        Nombre = nombreLimpio;
        NombreNormalizado = UniversidadAliada.NormalizarClaveTexto(nombreLimpio);
        MontoPorPeriodo = new Dinero(montoPorPeriodo.Value, string.IsNullOrWhiteSpace(moneda) ? "USD" : moneda);
        ModalidadPago = modalidadPago.Value;
        Alcance = alcance.Value;
        PermiteAcumulacion = alcance.Value == AlcancePrograma.PARCIAL && permiteAcumulacion;

        IncrementarVersion();
    }

    public AsociacionDonantePrograma AsociarDonante(Guid donanteId, DateTimeOffset ahoraUtc)
    {
        if (donanteId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador del donante es obligatorio.", "DONANTE_OBLIGATORIO");
        }

        if (_donantes.Any(d => d.DonanteId == donanteId))
        {
            throw new ExcepcionDeDominio(MensajeDonanteDuplicado, "DONANTE_YA_ASOCIADO");
        }

        var asociacion = new AsociacionDonantePrograma(Guid.NewGuid(), Id, donanteId, ahoraUtc);
        _donantes.Add(asociacion);
        IncrementarVersion();
        return asociacion;
    }

    public void ReemplazarRestricciones(
        IEnumerable<(Guid UniversidadId, string NombreUniversidad, string? CarreraNombre)> restricciones)
    {
        _restricciones.Clear();

        var vistas = new HashSet<(Guid, string)>();
        foreach (var item in restricciones)
        {
            var carreraNormalizada = string.IsNullOrWhiteSpace(item.CarreraNombre)
                ? string.Empty
                : UniversidadAliada.NormalizarClaveTexto(item.CarreraNombre);

            if (!vistas.Add((item.UniversidadId, carreraNormalizada)))
            {
                continue;
            }

            _restricciones.Add(new RestriccionElegibilidad(
                Guid.NewGuid(),
                Id,
                item.UniversidadId,
                item.NombreUniversidad,
                string.IsNullOrWhiteSpace(item.CarreraNombre) ? null : UniversidadAliada.LimpiarNombreVisible(item.CarreraNombre)));
        }

        IncrementarVersion();
    }

    public void ConfigurarCondiciones(CondicionesPermanencia condiciones)
    {
        ArgumentNullException.ThrowIfNull(condiciones);
        Condiciones = condiciones;
        IncrementarVersion();
    }
}
