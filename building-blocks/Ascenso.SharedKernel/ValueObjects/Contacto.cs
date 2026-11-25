using System.Net.Mail;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.ValueObjects;

public sealed record Contacto
{
    public string Correo { get; }
    public string Telefono { get; }

    public Contacto(string correo, string telefono)
    {
        var correoLimpio = (correo ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(correoLimpio) || !EsCorreoValido(correoLimpio))
        {
            throw new ExcepcionDeDominio("El correo electrónico de contacto no es válido.", "CORREO_INVALIDO");
        }

        var telefonoLimpio = (telefono ?? string.Empty).Trim();
        var soloDigitos = new string(telefonoLimpio.Where(char.IsDigit).ToArray());
        if (soloDigitos.Length < 7 || soloDigitos.Length > 15)
        {
            throw new ExcepcionDeDominio("El teléfono de contacto debe contener entre 7 y 15 dígitos.", "TELEFONO_INVALIDO");
        }

        Correo = correoLimpio;
        Telefono = telefonoLimpio;
    }

    private static bool EsCorreoValido(string correo)
    {
        try
        {
            var addr = new MailAddress(correo);
            return string.Equals(addr.Address, correo, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
