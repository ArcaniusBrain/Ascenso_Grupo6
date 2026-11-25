using System.Security.Cryptography;
using System.Text;
using Ascenso.SharedKernel.Seguridad;
using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S1.Identidad.Domain.Agregados;

public enum ResultadoConsumoOtp
{
    CONSUMIDO = 1,
    CONSUMIDO_IDEMPOTENTE = 2,
    CODIGO_INCORRECTO = 3,
    BLOQUEO_POR_INTENTOS = 4,
    EXPIRADO = 5,
    VINCULO_NO_COINCIDE = 6,
    RECHAZADO_REUSO_O_INVALIDADO = 7
}

/// <summary>
/// Desafío de código de un solo uso (OTP) por operación sensible en S1 (ADR-10, DDD-10, R4–R6, R42).
/// Independiente del 2FA de inicio de sesión (Anexo A HU-37.2).
/// Vinculado a (UsuarioId, Operacion, RecursoId, ChallengeId) con consumo único atómico protegido por Version (ADR-25).
/// </summary>
public sealed class DesafioOtpOperacion : RaizDeAgregado<Guid>
{
    public const int VigenciaMinutosPorDefecto = 15;
    public const int MaximoIntentosFallidos = 3;
    public const int HorasBloqueoTrasTresFallos = 1;
    public const string MensajeCodigoNoValido = "El código no es válido.";

    public Guid UsuarioId { get; private set; }
    public TipoOperacionSensible Operacion { get; private set; }
    public string RecursoId { get; private set; } = string.Empty;
    public CanalCodigo Canal { get; private set; }
    public string CodigoHash { get; private set; } = string.Empty;
    public string Salt { get; private set; } = string.Empty;
    public DateTimeOffset CreadoEnUtc { get; private set; }
    public DateTimeOffset ExpiraEnUtc { get; private set; }
    public int IntentosFallidos { get; private set; }
    public DateTimeOffset? ConsumidoEnUtc { get; private set; }
    public Guid? OperationIdConsumidor { get; private set; }
    public DateTimeOffset? InvalidadoEnUtc { get; private set; }

    private DesafioOtpOperacion()
    {
    }

    private DesafioOtpOperacion(
        Guid id,
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string recursoId,
        CanalCodigo canal,
        string codigoHash,
        string salt,
        DateTimeOffset creadoEnUtc,
        DateTimeOffset expiraEnUtc) : base(id)
    {
        UsuarioId = usuarioId;
        Operacion = operacion;
        RecursoId = recursoId;
        Canal = canal;
        CodigoHash = codigoHash;
        Salt = salt;
        CreadoEnUtc = creadoEnUtc;
        ExpiraEnUtc = expiraEnUtc;
        IntentosFallidos = 0;
    }

    public static DesafioOtpOperacion Crear(
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string? recursoId,
        CanalCodigo canal,
        string codigoPlano,
        DateTimeOffset ahoraUtc,
        int vigenciaMinutos = VigenciaMinutosPorDefecto,
        Guid? id = null)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El usuario del desafío OTP es obligatorio.", "OTP_USUARIO_INVALIDO");
        }

        if (!Enum.IsDefined(operacion))
        {
            throw new ExcepcionDeDominio("La operación sensible indicada no es válida.", "OTP_OPERACION_INVALIDA");
        }

        var recursoLimpio = NormalizarRecurso(recursoId);
        if (recursoLimpio.Length == 0)
        {
            throw new ExcepcionDeDominio("El identificador del recurso es obligatorio para emitir un OTP.", "OTP_RECURSO_OBLIGATORIO");
        }

        if (!Enum.IsDefined(canal))
        {
            throw new ExcepcionDeDominio("El canal de envío del OTP no es válido.", "OTP_CANAL_INVALIDO");
        }

        var codigoLimpio = (codigoPlano ?? string.Empty).Trim();
        if (codigoLimpio.Length < 6)
        {
            throw new ExcepcionDeDominio("El código OTP debe tener al menos 6 caracteres.", "OTP_CODIGO_INVALIDO");
        }

        if (vigenciaMinutos <= 0)
        {
            throw new ExcepcionDeDominio("La vigencia del OTP debe ser mayor que cero.", "OTP_VIGENCIA_INVALIDA");
        }

        var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var hash = CalcularHash(codigoLimpio, salt);
        var ahora = ahoraUtc.ToUniversalTime();

        return new DesafioOtpOperacion(
            id ?? Guid.NewGuid(),
            usuarioId,
            operacion,
            recursoLimpio,
            canal,
            hash,
            salt,
            ahora,
            ahora.AddMinutes(vigenciaMinutos));
    }

    public static string NormalizarRecurso(string? recursoId) =>
        (recursoId ?? string.Empty).Trim();

    public ResultadoConsumoOtp ValidarYConsumir(
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string? recursoId,
        string? codigoIngresado,
        Guid operationId,
        DateTimeOffset ahoraUtc)
    {
        if (operationId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El operationId es obligatorio para consumir un OTP.", "OTP_OPERATION_ID_OBLIGATORIO");
        }

        var recursoNormalizado = NormalizarRecurso(recursoId);
        var coincideVinculo = UsuarioId == usuarioId
            && Operacion == operacion
            && string.Equals(RecursoId, recursoNormalizado, StringComparison.OrdinalIgnoreCase);

        if (!coincideVinculo)
        {
            return ResultadoConsumoOtp.VINCULO_NO_COINCIDE;
        }

        // Idempotencia ante corte de red: mismo operationId que consumió originalmente (v1.3 §8.2)
        // vs nuevo intento con distinto operationId tras fallo de negocio: prohibido reutilizar (R42)
        if (ConsumidoEnUtc is not null)
        {
            return OperationIdConsumidor == operationId
                ? ResultadoConsumoOtp.CONSUMIDO_IDEMPOTENTE
                : ResultadoConsumoOtp.RECHAZADO_REUSO_O_INVALIDADO;
        }

        if (InvalidadoEnUtc is not null)
        {
            return ResultadoConsumoOtp.RECHAZADO_REUSO_O_INVALIDADO;
        }

        var ahora = ahoraUtc.ToUniversalTime();
        if (ahora > ExpiraEnUtc)
        {
            return ResultadoConsumoOtp.EXPIRADO;
        }

        var codigoLimpio = (codigoIngresado ?? string.Empty).Trim();
        var hashIngresado = CalcularHash(codigoLimpio, Salt);
        var coincideCodigo = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hashIngresado),
            Encoding.UTF8.GetBytes(CodigoHash));

        if (coincideCodigo)
        {
            ConsumidoEnUtc = ahora;
            OperationIdConsumidor = operationId;
            IncrementarVersion();
            return ResultadoConsumoOtp.CONSUMIDO;
        }

        IntentosFallidos++;
        IncrementarVersion();

        if (IntentosFallidos >= MaximoIntentosFallidos)
        {
            InvalidadoEnUtc = ahora;
            return ResultadoConsumoOtp.BLOQUEO_POR_INTENTOS;
        }

        return ResultadoConsumoOtp.CODIGO_INCORRECTO;
    }

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
        return Convert.ToHexString(SHA256.HashData(datos));
    }
}

/// <summary>
/// Bloqueo temporal de 1 hora acotado a la terna (UsuarioId, Operacion, RecursoId)
/// tras 3 intentos fallidos de código OTP (HU-20.1 Escenario 13 y FA-9, R4c).
/// </summary>
public sealed class BloqueoOtpOperacion : RaizDeAgregado<Guid>
{
    public Guid UsuarioId { get; private set; }
    public TipoOperacionSensible Operacion { get; private set; }
    public string RecursoId { get; private set; } = string.Empty;
    public DateTimeOffset BloqueadoHastaUtc { get; private set; }

    private BloqueoOtpOperacion()
    {
    }

    private BloqueoOtpOperacion(
        Guid id,
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string recursoId,
        DateTimeOffset bloqueadoHastaUtc) : base(id)
    {
        UsuarioId = usuarioId;
        Operacion = operacion;
        RecursoId = recursoId;
        BloqueadoHastaUtc = bloqueadoHastaUtc;
    }

    public static BloqueoOtpOperacion Crear(
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string recursoId,
        DateTimeOffset bloqueadoHastaUtc,
        Guid? id = null) =>
        new(
            id ?? Guid.NewGuid(),
            usuarioId,
            operacion,
            DesafioOtpOperacion.NormalizarRecurso(recursoId),
            bloqueadoHastaUtc.ToUniversalTime());

    public bool EstaVigente(DateTimeOffset ahoraUtc) =>
        ahoraUtc.ToUniversalTime() < BloqueadoHastaUtc;

    public void ExtenderHasta(DateTimeOffset nuevoHastaUtc)
    {
        BloqueadoHastaUtc = nuevoHastaUtc.ToUniversalTime();
        IncrementarVersion();
    }
}
