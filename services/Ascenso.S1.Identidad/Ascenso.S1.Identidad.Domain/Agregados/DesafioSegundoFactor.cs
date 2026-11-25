using System.Security.Cryptography;
using System.Text;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S1.Identidad.Domain.Agregados;

public enum ResultadoValidacionSegundoFactor
{
    VALIDO = 1,
    CODIGO_INCORRECTO = 2,
    BLOQUEO_POR_INTENTOS = 3,
    EXPIRADO = 4,
    YA_USADO_O_INVALIDADO = 5
}

/// <summary>
/// Desafío de segundo factor de inicio de sesión controlado por S1 (HU-37.2, R4a–R4f, ADR-09, C14).
/// - Código almacenado únicamente como hash SHA-256 + salt (nunca en texto plano).
/// - Vigencia: 15 minutos por defecto (R4b, HU-37.2 Escenario 9).
/// - Máximo 3 intentos fallidos -> bloqueo de 1 hora (R4c, HU-37.2 Escenario 10).
/// - Consumo de un solo uso y reemplazo/invalidación al reenviar (C14).
/// </summary>
public sealed class DesafioSegundoFactor : RaizDeAgregado<Guid>
{
    public const int VigenciaMinutosPorDefecto = 15;
    public const int MaximoIntentosFallidos = 3;
    public const int HorasBloqueoTrasTresFallos = 1;
    public const string MensajeCodigoInvalido = "El segundo factor no es válido.";

    public Guid UsuarioId { get; private set; }
    public CanalCodigo Canal { get; private set; }
    public string CodigoHash { get; private set; } = string.Empty;
    public string Salt { get; private set; } = string.Empty;
    public DateTimeOffset CreadoEnUtc { get; private set; }
    public DateTimeOffset ExpiraEnUtc { get; private set; }
    public int IntentosFallidos { get; private set; }
    public DateTimeOffset? ConsumidoEnUtc { get; private set; }
    public DateTimeOffset? InvalidadoEnUtc { get; private set; }
    public bool EsRecuperacionCorreo { get; private set; }

    private DesafioSegundoFactor()
    {
    }

    private DesafioSegundoFactor(
        Guid id,
        Guid usuarioId,
        CanalCodigo canal,
        string codigoHash,
        string salt,
        DateTimeOffset creadoEnUtc,
        DateTimeOffset expiraEnUtc,
        bool esRecuperacionCorreo) : base(id)
    {
        UsuarioId = usuarioId;
        Canal = canal;
        CodigoHash = codigoHash;
        Salt = salt;
        CreadoEnUtc = creadoEnUtc;
        ExpiraEnUtc = expiraEnUtc;
        IntentosFallidos = 0;
        EsRecuperacionCorreo = esRecuperacionCorreo;
    }

    public static DesafioSegundoFactor Crear(
        Guid usuarioId,
        CanalCodigo canal,
        string codigoPlano,
        DateTimeOffset ahoraUtc,
        int vigenciaMinutos = VigenciaMinutosPorDefecto,
        bool esRecuperacionCorreo = false,
        Guid? id = null)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El usuario del desafío 2FA es obligatorio.", "DESAFIO_USUARIO_INVALIDO");
        }

        var limpio = (codigoPlano ?? string.Empty).Trim();
        if (limpio.Length < 6)
        {
            throw new ExcepcionDeDominio("El código de verificación debe tener al menos 6 caracteres.", "CODIGO_2FA_INVALIDO");
        }

        if (vigenciaMinutos <= 0)
        {
            throw new ExcepcionDeDominio("La vigencia del código debe ser mayor que cero.", "VIGENCIA_INVALIDA");
        }

        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = CalcularHash(limpio, salt);
        var ahora = ahoraUtc.ToUniversalTime();

        return new DesafioSegundoFactor(
            id ?? Guid.NewGuid(),
            usuarioId,
            canal,
            hash,
            salt,
            ahora,
            ahora.AddMinutes(vigenciaMinutos),
            esRecuperacionCorreo);
    }

    public bool EstaActivo(DateTimeOffset ahoraUtc) =>
        ConsumidoEnUtc is null
        && InvalidadoEnUtc is null
        && ahoraUtc.ToUniversalTime() <= ExpiraEnUtc;

    public ResultadoValidacionSegundoFactor ValidarYConsumir(string? codigoIngresado, DateTimeOffset ahoraUtc)
    {
        var ahora = ahoraUtc.ToUniversalTime();

        if (ConsumidoEnUtc is not null || InvalidadoEnUtc is not null)
        {
            return ResultadoValidacionSegundoFactor.YA_USADO_O_INVALIDADO;
        }

        if (ahora > ExpiraEnUtc)
        {
            return ResultadoValidacionSegundoFactor.EXPIRADO;
        }

        var limpio = (codigoIngresado ?? string.Empty).Trim();
        var hashIngresado = CalcularHash(limpio, Salt);

        var coincide = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hashIngresado),
            Encoding.UTF8.GetBytes(CodigoHash));

        if (coincide)
        {
            ConsumidoEnUtc = ahora;
            IncrementarVersion();
            return ResultadoValidacionSegundoFactor.VALIDO;
        }

        IntentosFallidos++;
        IncrementarVersion();

        if (IntentosFallidos >= MaximoIntentosFallidos)
        {
            InvalidadoEnUtc = ahora;
            return ResultadoValidacionSegundoFactor.BLOQUEO_POR_INTENTOS;
        }

        return ResultadoValidacionSegundoFactor.CODIGO_INCORRECTO;
    }

    /// <summary>
    /// Invalida el desafío previo al solicitar reenvío o recuperación (C14).
    /// </summary>
    public void Invalidar(DateTimeOffset ahoraUtc)
    {
        if (ConsumidoEnUtc is not null || InvalidadoEnUtc is not null)
        {
            return;
        }

        InvalidadoEnUtc = ahoraUtc.ToUniversalTime();
        IncrementarVersion();
    }

    private static string CalcularHash(string codigo, string salt)
    {
        var datos = Encoding.UTF8.GetBytes($"{salt}:{codigo}");
        var hash = SHA256.HashData(datos);
        return Convert.ToHexString(hash);
    }
}
