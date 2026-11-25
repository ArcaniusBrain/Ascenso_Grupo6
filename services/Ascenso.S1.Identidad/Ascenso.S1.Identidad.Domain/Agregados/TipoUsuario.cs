using Ascenso.S1.Identidad.Domain.Servicios;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S1.Identidad.Domain.Agregados;

/// <summary>
/// Catálogo configurable de tipos de usuario y sus permisos (DDD v3.3 §13, HU-37.1).
/// Agregar un tipo nuevo no exige cambios de código ni redespliegue (Anexo A HU-37.1).
/// </summary>
public sealed class TipoUsuario : RaizDeAgregado<Guid>
{
    public const string MensajeNombreObligatorio = "Escribe el nombre del tipo de usuario.";

    private readonly HashSet<string> _permisos = new(StringComparer.OrdinalIgnoreCase);

    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;
    public bool AccesoFinancieroODecision { get; private set; }
    public bool EsEnlaceUniversidad { get; private set; }
    public bool EsPredefinido { get; private set; }
    public IReadOnlyCollection<string> Permisos => _permisos;

    private TipoUsuario()
    {
    }

    private TipoUsuario(
        Guid id,
        string nombre,
        IEnumerable<string> permisos,
        bool accesoFinancieroODecision,
        bool esEnlaceUniversidad,
        bool esPredefinido) : base(id)
    {
        Nombre = nombre;
        NombreNormalizado = NormalizarNombre(nombre);
        EsEnlaceUniversidad = esEnlaceUniversidad;
        EsPredefinido = esPredefinido;

        foreach (var permiso in permisos)
        {
            var limpio = (permiso ?? string.Empty).Trim();
            if (limpio.Length > 0)
            {
                _permisos.Add(limpio);
            }
        }

        AccesoFinancieroODecision = accesoFinancieroODecision
            || CatalogoPermisos.AlgunoImplicaAccesoFinancieroODecision(_permisos);
    }

    public static TipoUsuario Crear(
        string? nombre,
        IEnumerable<string>? permisos,
        bool accesoFinancieroODecision = false,
        bool esEnlaceUniversidad = false,
        bool esPredefinido = false,
        Guid? id = null)
    {
        var limpio = (nombre ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            throw new ExcepcionDeDominio(MensajeNombreObligatorio, "TIPO_USUARIO_NOMBRE_OBLIGATORIO");
        }

        return new TipoUsuario(
            id ?? Guid.NewGuid(),
            limpio,
            permisos ?? Array.Empty<string>(),
            accesoFinancieroODecision,
            esEnlaceUniversidad,
            esPredefinido);
    }

    public static string MensajeNombreDuplicado(string nombre) =>
        $"El tipo de usuario {nombre.Trim()} ya existe.";

    public static string NormalizarNombre(string nombre) =>
        (nombre ?? string.Empty).Trim().ToUpperInvariant();

    public void OtorgarPermiso(string permiso)
    {
        var limpio = (permiso ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            throw new ExcepcionDeDominio("El código del permiso es obligatorio.", "PERMISO_INVALIDO");
        }

        if (_permisos.Add(limpio))
        {
            if (CatalogoPermisos.ImplicaAccesoFinancieroODecision(limpio))
            {
                AccesoFinancieroODecision = true;
            }

            IncrementarVersion();
        }
    }

    public void ActualizarPermisos(IEnumerable<string> nuevosPermisos, bool? accesoFinancieroExplicito = null)
    {
        ArgumentNullException.ThrowIfNull(nuevosPermisos);

        _permisos.Clear();
        foreach (var permiso in nuevosPermisos)
        {
            var limpio = (permiso ?? string.Empty).Trim();
            if (limpio.Length > 0)
            {
                _permisos.Add(limpio);
            }
        }

        AccesoFinancieroODecision = (accesoFinancieroExplicito ?? false)
            || CatalogoPermisos.AlgunoImplicaAccesoFinancieroODecision(_permisos);

        IncrementarVersion();
    }

    /// <summary>
    /// Resuelve dinámicamente si este tipo de usuario exige verificación en dos pasos (HU-37.1 E11, HU-37.2 E4 y E11).
    /// Tipos sin acceso financiero ni de decisión y que no son enlace universitario quedan exentos (C14).
    /// </summary>
    public bool ExigeSegundoFactor() => AccesoFinancieroODecision || EsEnlaceUniversidad;

    public bool TienePermiso(string permiso) =>
        !string.IsNullOrWhiteSpace(permiso) && _permisos.Contains(permiso.Trim());
}
