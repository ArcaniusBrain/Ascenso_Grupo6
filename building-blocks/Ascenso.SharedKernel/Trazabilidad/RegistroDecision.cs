using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;

namespace Ascenso.SharedKernel.Trazabilidad;

/// <summary>
/// Registro inmutable (append-only) de toda decisión relevante del sistema (DDD v3.3 §14, D-6, RF-35).
/// No es una raíz de agregado ni tiene repositorio propio: se guarda en la misma transacción local
/// que el agregado que registró el evento y el Outbox (ADR-12).
/// </summary>
public sealed class RegistroDecision
{
    public const string MensajeJustificacionObligatoria = "Escribe la justificación de esta decisión.";
    public const string MensajeFalloPersistencia = "No se pudo registrar la decisión. No se aplicó ningún cambio.";
    public const string JustificacionInactivacionNoRenovacion =
        "Inactivación automática por cierre de ventana de renovación sin solicitud.";
    public const string JustificacionInactivacionPausaVencida =
        "Inactivación automática por vencimiento de pausa sin reactivación.";

    public Guid Id { get; private set; }
    public TipoDecision Tipo { get; private set; }
    public SujetoRef Sujeto { get; private set; } = null!;
    public Guid? ProgramaId { get; private set; }
    public Guid? PropietarioUsuarioId { get; private set; }
    public ActorDecision Actor { get; private set; } = null!;
    public DateTimeOffset OcurridoEnUtc { get; private set; }
    public Justificacion Justificacion { get; private set; } = null!;
    public string? EstadoAnterior { get; private set; }
    public string? EstadoNuevo { get; private set; }
    public string? DatosJson { get; private set; }
    public string ServicioOrigen { get; private set; } = string.Empty;
    public Guid CorrelationId { get; private set; }

    private RegistroDecision()
    {
    }

    /// <summary>
    /// Crea un registro inmutable para una decisión tomada por un usuario con poder de decisión (HU-35.1).
    /// </summary>
    public static RegistroDecision Crear(
        TipoDecision tipo,
        SujetoRef sujeto,
        ActorDecision actor,
        DateTimeOffset ocurridoEnUtc,
        string? justificacionTexto,
        string servicioOrigen,
        Guid correlationId,
        Guid? programaId = null,
        Guid? propietarioUsuarioId = null,
        string? estadoAnterior = null,
        string? estadoNuevo = null,
        string? datosJson = null,
        Guid? id = null)
    {
        ArgumentNullException.ThrowIfNull(sujeto);
        ArgumentNullException.ThrowIfNull(actor);

        if (actor.EsSistema)
        {
            throw new ExcepcionDeDominio(
                "Las decisiones automáticas del sistema deben crearse mediante CrearAutomatica.",
                "ACTOR_SISTEMA_NO_PERMITIDO_EN_CREAR");
        }

        if (string.IsNullOrWhiteSpace(justificacionTexto))
        {
            throw new ExcepcionDeDominio(MensajeJustificacionObligatoria, "JUSTIFICACION_OBLIGATORIA");
        }

        Justificacion justificacion;
        try
        {
            justificacion = new Justificacion(justificacionTexto);
        }
        catch (ExcepcionDeDominio)
        {
            throw new ExcepcionDeDominio(MensajeJustificacionObligatoria, "JUSTIFICACION_OBLIGATORIA");
        }

        return ConstruirInterno(
            id ?? Guid.NewGuid(),
            tipo,
            sujeto,
            programaId,
            propietarioUsuarioId,
            actor,
            ocurridoEnUtc,
            justificacion,
            estadoAnterior,
            estadoNuevo,
            datosJson,
            servicioOrigen,
            correlationId);
    }

    /// <summary>
    /// Crea un registro inmutable para una decisión automática del sistema
    /// (por ejemplo, entrada automática a Inactiva por cierre de ventana o pausa vencida).
    /// </summary>
    public static RegistroDecision CrearAutomatica(
        TipoDecision tipo,
        SujetoRef sujeto,
        DateTimeOffset ocurridoEnUtc,
        string justificacionAutomatica,
        string servicioOrigen,
        Guid correlationId,
        Guid? programaId = null,
        Guid? propietarioUsuarioId = null,
        string? estadoAnterior = null,
        string? estadoNuevo = null,
        string? datosJson = null,
        string rolSistema = "SISTEMA",
        Guid? id = null)
    {
        ArgumentNullException.ThrowIfNull(sujeto);

        if (string.IsNullOrWhiteSpace(justificacionAutomatica))
        {
            throw new ExcepcionDeDominio(
                "Toda decisión automática debe incluir su justificación técnica descriptiva.",
                "JUSTIFICACION_AUTOMATICA_OBLIGATORIA");
        }

        var justificacion = new Justificacion(justificacionAutomatica);

        return ConstruirInterno(
            id ?? Guid.NewGuid(),
            tipo,
            sujeto,
            programaId,
            propietarioUsuarioId,
            ActorDecision.Sistema(rolSistema),
            ocurridoEnUtc,
            justificacion,
            estadoAnterior,
            estadoNuevo,
            datosJson,
            servicioOrigen,
            correlationId);
    }

    private static RegistroDecision ConstruirInterno(
        Guid id,
        TipoDecision tipo,
        SujetoRef sujeto,
        Guid? programaId,
        Guid? propietarioUsuarioId,
        ActorDecision actor,
        DateTimeOffset ocurridoEnUtc,
        Justificacion justificacion,
        string? estadoAnterior,
        string? estadoNuevo,
        string? datosJson,
        string servicioOrigen,
        Guid correlationId)
    {
        if (id == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador del registro de decisión es obligatorio.", "REGISTRO_ID_INVALIDO");
        }

        if (!Enum.IsDefined(tipo))
        {
            throw new ExcepcionDeDominio("El tipo de decisión no es válido.", "TIPO_DECISION_INVALIDO");
        }

        var servicioLimpio = (servicioOrigen ?? string.Empty).Trim();
        if (servicioLimpio.Length == 0)
        {
            throw new ExcepcionDeDominio("El servicio de origen es obligatorio.", "SERVICIO_ORIGEN_OBLIGATORIO");
        }

        if (correlationId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El CorrelationId es obligatorio para trazabilidad.", "CORRELATION_ID_OBLIGATORIO");
        }

        return new RegistroDecision
        {
            Id = id,
            Tipo = tipo,
            Sujeto = sujeto,
            ProgramaId = programaId,
            PropietarioUsuarioId = propietarioUsuarioId,
            Actor = actor,
            OcurridoEnUtc = ocurridoEnUtc.ToUniversalTime(),
            Justificacion = justificacion,
            EstadoAnterior = NormalizarOpcional(estadoAnterior),
            EstadoNuevo = NormalizarOpcional(estadoNuevo),
            DatosJson = NormalizarOpcional(datosJson),
            ServicioOrigen = servicioLimpio,
            CorrelationId = correlationId
        };
    }

    private static string? NormalizarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
