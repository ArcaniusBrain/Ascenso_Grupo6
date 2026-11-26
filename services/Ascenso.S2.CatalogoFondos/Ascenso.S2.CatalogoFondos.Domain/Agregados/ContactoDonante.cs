using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Value Object con la información de contacto de un donante (HU-13.1 E1-E3, C18).
/// Exige al menos correo válido o teléfono no vacío.
/// </summary>
public sealed record ContactoDonante
{
    public const string MensajeContactoObligatorio = "Indica los datos de contacto del donante.";

    public string? Correo { get; private set; }
    public string? Telefono { get; private set; }

    private ContactoDonante()
    {
    }

    public ContactoDonante(string? correo, string? telefono)
    {
        var correoLimpio = string.IsNullOrWhiteSpace(correo) ? null : correo.Trim().ToLowerInvariant();
        var telefonoLimpio = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();

        if (correoLimpio is null && telefonoLimpio is null)
        {
            throw new ExcepcionDeDominio(MensajeContactoObligatorio, "CONTACTO_DONANTE_OBLIGATORIO");
        }

        if (correoLimpio is not null
            && (!correoLimpio.Contains('@') || correoLimpio.StartsWith('@') || correoLimpio.EndsWith('@')))
        {
            throw new ExcepcionDeDominio(
                "El correo del donante no tiene un formato válido.",
                "CORREO_DONANTE_INVALIDO");
        }

        Correo = correoLimpio;
        Telefono = telefonoLimpio;
    }
}
