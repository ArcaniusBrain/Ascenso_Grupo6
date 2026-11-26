using System.Text.RegularExpressions;
using Ascenso.S2.CatalogoFondos.Domain.Eventos;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Raíz de agregado UniversidadAliada (RF-14 · HU-14.1, HU-14.2, DDD v3.3 §7, ADR-25).
/// Protege: nombre único normalizado, convenio y contacto obligatorios, carreras aisladas por universidad
/// y costos de matrícula append-only (vigente = último registro por VersionRegistro).
/// </summary>
public sealed partial class UniversidadAliada : RaizDeAgregado<Guid>
{
    public const string MensajeNombreCarreraObligatorio = "Indica el nombre de la carrera.";
    public const string MensajeSinCarreras = "Esta universidad no tiene carreras elegibles registradas.";
    public const string MensajeCostoDebeSerPositivo = "El costo de matrícula debe ser mayor que cero.";
    public const string MensajeSinCostoEnPeriodo = "Sin costo registrado para este periodo";

    private readonly List<CarreraElegible> _carreras = [];
    private readonly List<CostoMatricula> _costos = [];

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public Convenio Convenio { get; private set; } = default!;
    public ContactoDesignado ContactoDesignado { get; private set; } = default!;
    public DateTimeOffset RegistradaEnUtc { get; private set; }

    public IReadOnlyCollection<CarreraElegible> Carreras => _carreras.AsReadOnly();
    public IReadOnlyCollection<CostoMatricula> Costos => _costos.AsReadOnly();

    private UniversidadAliada()
    {
    }

    private UniversidadAliada(
        Guid id,
        string nombre,
        string nombreNormalizado,
        Convenio convenio,
        ContactoDesignado contactoDesignado,
        DateTimeOffset registradaEnUtc) : base(id)
    {
        Nombre = nombre;
        NombreNormalizado = nombreNormalizado;
        Convenio = convenio;
        ContactoDesignado = contactoDesignado;
        RegistradaEnUtc = registradaEnUtc;
    }

    public static UniversidadAliada Crear(
        string? nombre,
        string? terminosConvenio,
        ViaDatosAcademicos viaDatosAcademicos,
        ContactoDesignado? contactoDesignado,
        DateTimeOffset ahoraUtc)
    {
        var nombreLimpio = LimpiarNombreVisible(nombre);
        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            throw new ExcepcionDeDominio(
                "Indica el nombre de la universidad aliada.",
                "NOMBRE_UNIVERSIDAD_OBLIGATORIO");
        }

        var convenio = new Convenio(terminosConvenio, viaDatosAcademicos);
        if (contactoDesignado is null)
        {
            throw new ExcepcionDeDominio(
                ContactoDesignado.MensajeContactoObligatorio,
                "CONTACTO_DESIGNADO_OBLIGATORIO");
        }

        var universidad = new UniversidadAliada(
            Guid.NewGuid(),
            nombreLimpio,
            NormalizarClaveTexto(nombreLimpio),
            convenio,
            contactoDesignado,
            ahoraUtc);

        universidad.Registrar(new UniversidadRegistradaEvento(
            universidad.Id,
            universidad.Nombre,
            universidad.Convenio.Via.ToString(),
            ahoraUtc));

        return universidad;
    }

    /// <summary>
    /// Actualiza nombre (permitiendo corregir mayúsculas/minúsculas, C16), términos del convenio, vía académica y contacto.
    /// </summary>
    public void ActualizarDatos(
        string? nuevoNombre,
        string? terminosConvenio,
        ViaDatosAcademicos viaDatosAcademicos,
        ContactoDesignado? contactoDesignado)
    {
        var nombreLimpio = LimpiarNombreVisible(nuevoNombre);
        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            throw new ExcepcionDeDominio(
                "Indica el nombre de la universidad aliada.",
                "NOMBRE_UNIVERSIDAD_OBLIGATORIO");
        }

        var convenio = new Convenio(terminosConvenio, viaDatosAcademicos);
        if (contactoDesignado is null)
        {
            throw new ExcepcionDeDominio(
                ContactoDesignado.MensajeContactoObligatorio,
                "CONTACTO_DESIGNADO_OBLIGATORIO");
        }

        Nombre = nombreLimpio;
        NombreNormalizado = NormalizarClaveTexto(nombreLimpio);
        Convenio = convenio;
        ContactoDesignado = contactoDesignado;
        IncrementarVersion();
    }

    public CarreraElegible AgregarCarrera(string? nombreCarrera)
    {
        var nombreLimpio = LimpiarNombreVisible(nombreCarrera);
        if (string.IsNullOrWhiteSpace(nombreLimpio))
        {
            throw new ExcepcionDeDominio(
                MensajeNombreCarreraObligatorio,
                "NOMBRE_CARRERA_VACIO");
        }

        var normalizado = NormalizarClaveTexto(nombreLimpio);
        if (_carreras.Any(c => c.NombreNormalizado == normalizado))
        {
            throw new ExcepcionDeDominio(
                $"La carrera '{nombreLimpio}' ya está registrada en {Nombre}.",
                "CARRERA_DUPLICADA");
        }

        var carrera = new CarreraElegible(Guid.NewGuid(), Id, nombreLimpio, normalizado);
        _carreras.Add(carrera);
        IncrementarVersion();
        return carrera;
    }

    public CostoMatricula RegistrarCosto(
        CodigoPeriodo periodo,
        decimal monto,
        string? moneda,
        DateTimeOffset ahoraUtc)
    {
        if (monto <= 0m)
        {
            throw new ExcepcionDeDominio(
                MensajeCostoDebeSerPositivo,
                "COSTO_MATRICULA_INVALIDO");
        }

        var codigoMoneda = string.IsNullOrWhiteSpace(moneda) ? "USD" : moneda;
        var dinero = new Dinero(monto, codigoMoneda);

        var siguienteVersion = _costos
            .Where(c => c.Periodo == periodo)
            .Select(c => c.VersionRegistro)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var registro = new CostoMatricula(
            Guid.NewGuid(),
            Id,
            periodo,
            dinero,
            siguienteVersion,
            ahoraUtc);

        _costos.Add(registro);
        IncrementarVersion();
        Registrar(new CostoMatriculaRegistradoEvento(
            Id,
            periodo.Valor,
            dinero.Cantidad,
            dinero.Moneda,
            siguienteVersion,
            ahoraUtc));

        return registro;
    }

    public CostoMatricula? CostoVigente(CodigoPeriodo periodo) =>
        _costos
            .Where(c => c.Periodo == periodo)
            .OrderByDescending(c => c.VersionRegistro)
            .ThenByDescending(c => c.RegistradoEnUtc)
            .FirstOrDefault();

    public IReadOnlyList<CostoMatricula> HistorialCostos(CodigoPeriodo periodo) =>
        _costos
            .Where(c => c.Periodo == periodo)
            .OrderBy(c => c.VersionRegistro)
            .ToArray();

    public IReadOnlyList<CostoMatricula> CostosVigentesPorPeriodo() =>
        _costos
            .GroupBy(c => c.Periodo)
            .Select(g => g.OrderByDescending(x => x.VersionRegistro).First())
            .OrderBy(c => c.Periodo, ComparadorCodigoPeriodo.Instancia)
            .ToArray();

    public static string NormalizarClaveTexto(string? texto)
    {
        var limpio = LimpiarNombreVisible(texto);
        return limpio.ToUpperInvariant();
    }

    public static string LimpiarNombreVisible(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        return EspaciosMultiplesRegex().Replace(texto.Trim(), " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspaciosMultiplesRegex();
}
