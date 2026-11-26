using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Raíz de agregado Donante (RF-13 · HU-13.1, HU-13.2, DDD v3.3 §8, ADR-25).
/// Modela organizaciones y personas sin cuenta de acceso en la plataforma,
/// con un ledger append-only de aportes por programa y periodo.
/// </summary>
public sealed class Donante : RaizDeAgregado<Guid>
{
    public const string MensajeNombreObligatorio = "Indica el nombre del donante.";
    public const string MensajeTipoObligatorio = "Elige si el donante es una organización o una persona.";
    public const string MensajeMontoDebeSerPositivo = "El monto del aporte debe ser mayor que cero.";
    public const string MensajePeriodoObligatorio = "Indica el periodo del aporte.";
    public const string MensajeSinAportes = "Este donante no tiene aportes registrados.";

    private readonly List<Aporte> _aportes = [];

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public TipoDonante Tipo { get; private set; }
    public ContactoDonante Contacto { get; private set; } = default!;
    public DateTimeOffset RegistradoEnUtc { get; private set; }

    public IReadOnlyCollection<Aporte> Aportes => _aportes.AsReadOnly();

    private Donante()
    {
    }

    private Donante(
        Guid id,
        string nombre,
        string nombreNormalizado,
        TipoDonante tipo,
        ContactoDonante contacto,
        DateTimeOffset registradoEnUtc) : base(id)
    {
        Nombre = nombre;
        NombreNormalizado = nombreNormalizado;
        Tipo = tipo;
        Contacto = contacto;
        RegistradoEnUtc = registradoEnUtc;
    }

    public static Donante Crear(
        string? nombre,
        TipoDonante? tipo,
        ContactoDonante? contacto,
        DateTimeOffset ahoraUtc)
    {
        var nombreLimpio = UniversidadAliada.LimpiarNombreVisible(nombre);
        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            throw new ExcepcionDeDominio(MensajeNombreObligatorio, "NOMBRE_DONANTE_OBLIGATORIO");
        }

        if (tipo is null || !Enum.IsDefined(tipo.Value))
        {
            throw new ExcepcionDeDominio(MensajeTipoObligatorio, "TIPO_DONANTE_INVALIDO");
        }

        if (contacto is null)
        {
            throw new ExcepcionDeDominio(ContactoDonante.MensajeContactoObligatorio, "CONTACTO_DONANTE_OBLIGATORIO");
        }

        var donante = new Donante(
            Guid.NewGuid(),
            nombreLimpio,
            UniversidadAliada.NormalizarClaveTexto(nombreLimpio),
            tipo.Value,
            contacto,
            ahoraUtc);

        donante.Registrar(new DonanteRegistradoEvento(
            donante.Id,
            donante.Nombre,
            donante.Tipo.ToString(),
            ahoraUtc));

        return donante;
    }

    /// <summary>
    /// Registra un aporte append-only sin modificar ni eliminar aportes previos (HU-13.2 E1, E4, E7).
    /// </summary>
    public Aporte RegistrarAporte(
        Guid programaId,
        CodigoPeriodo? periodo,
        decimal monto,
        string? moneda,
        DateTimeOffset ahoraUtc)
    {
        if (programaId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica el programa del aporte.", "PROGRAMA_APORTE_OBLIGATORIO");
        }

        if (periodo is null)
        {
            throw new ExcepcionDeDominio(MensajePeriodoObligatorio, "PERIODO_APORTE_OBLIGATORIO");
        }

        if (monto <= 0m)
        {
            throw new ExcepcionDeDominio(MensajeMontoDebeSerPositivo, "MONTO_APORTE_INVALIDO");
        }

        var codigoMoneda = string.IsNullOrWhiteSpace(moneda) ? "USD" : moneda;
        var dinero = new Dinero(monto, codigoMoneda);

        var aporte = new Aporte(
            Guid.NewGuid(),
            Id,
            programaId,
            periodo,
            dinero,
            ahoraUtc);

        _aportes.Add(aporte);
        IncrementarVersion();
        Registrar(new AporteRegistradoEvento(
            aporte.Id,
            Id,
            Nombre,
            programaId,
            periodo.Valor,
            dinero.Cantidad,
            dinero.Moneda,
            ahoraUtc));

        return aporte;
    }

    public IReadOnlyList<Aporte> ObtenerHistorial(CodigoPeriodo? filtroPeriodo = null)
    {
        IEnumerable<Aporte> consulta = _aportes;
        if (filtroPeriodo is not null)
        {
            consulta = consulta.Where(a => a.Periodo == filtroPeriodo);
        }

        return consulta
            .OrderBy(a => a.Periodo, ComparadorCodigoPeriodo.Instancia)
            .ThenBy(a => a.RegistradoEnUtc)
            .ToArray();
    }

    public decimal TotalAportado(CodigoPeriodo? filtroPeriodo = null) =>
        ObtenerHistorial(filtroPeriodo).Sum(a => a.Monto.Cantidad);

    public static string MensajeSinAportesEnPeriodo(CodigoPeriodo periodo) =>
        $"No hay aportes de este donante en el periodo {periodo.Valor}.";
}
