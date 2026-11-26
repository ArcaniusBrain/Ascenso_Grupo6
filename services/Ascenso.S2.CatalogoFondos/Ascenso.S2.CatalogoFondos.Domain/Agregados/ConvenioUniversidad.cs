using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Value Object del convenio institucional de una universidad aliada (DDD v3.3 §7, HU-14.1).
/// </summary>
public sealed record Convenio
{
    public const string MensajeTerminosObligatorios = "Indica los términos del convenio.";

    public string Terminos { get; private set; } = string.Empty;
    public ViaDatosAcademicos Via { get; private set; }

    private Convenio()
    {
    }

    public Convenio(string? terminos, ViaDatosAcademicos via)
    {
        if (string.IsNullOrWhiteSpace(terminos))
        {
            throw new ExcepcionDeDominio(MensajeTerminosObligatorios, "TERMINOS_CONVENIO_OBLIGATORIOS");
        }

        if (!Enum.IsDefined(via))
        {
            throw new ExcepcionDeDominio(
                "La vía de intercambio de datos académicos debe ser API o MANUAL.",
                "VIA_DATOS_ACADEMICOS_INVALIDA");
        }

        Terminos = terminos.Trim();
        Via = via;
    }
}

/// <summary>
/// Value Object del contacto designado (enlace) de la universidad aliada (HU-14.1).
/// </summary>
public sealed record ContactoDesignado
{
    public const string MensajeContactoObligatorio = "Indica el contacto designado de la universidad.";

    public string NombreCompleto { get; private set; } = string.Empty;
    public string? Correo { get; private set; }
    public string? Telefono { get; private set; }

    private ContactoDesignado()
    {
    }

    public ContactoDesignado(string? nombreCompleto, string? correo = null, string? telefono = null)
    {
        if (string.IsNullOrWhiteSpace(nombreCompleto))
        {
            throw new ExcepcionDeDominio(MensajeContactoObligatorio, "CONTACTO_DESIGNADO_OBLIGATORIO");
        }

        NombreCompleto = nombreCompleto.Trim();

        if (!string.IsNullOrWhiteSpace(correo))
        {
            var correoLimpio = correo.Trim().ToLowerInvariant();
            if (!correoLimpio.Contains('@') || correoLimpio.StartsWith('@') || correoLimpio.EndsWith('@'))
            {
                throw new ExcepcionDeDominio(
                    "El correo del contacto designado no tiene un formato válido.",
                    "CORREO_CONTACTO_INVALIDO");
            }

            Correo = correoLimpio;
        }

        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
    }
}
