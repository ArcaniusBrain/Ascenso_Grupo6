namespace Ascenso.SharedKernel.Trazabilidad;

/// <summary>
/// Catálogo cerrado de tipos de decisión del modelo DDD v3.3 §14 (38 valores).
/// </summary>
public enum TipoDecision
{
    POSTULACION_RECIBIDA = 1,
    NO_ELEGIBLE = 2,
    ANULACION_NO_ELEGIBLE = 3,
    RETIRO = 4,
    ASIGNACION_EVALUADOR = 5,
    EXCEPCION_UN_EVALUADOR = 6,
    CONFLICTO_DECLARADO = 7,
    CONSOLIDACION = 8,
    CASO_LIMITE_RESUELTO = 9,
    LISTA_ESPERA = 10,
    OFERTA_EMITIDA = 11,
    OFERTA_ACEPTADA = 12,
    OFERTA_RECHAZADA = 13,
    OFERTA_EXPIRADA = 14,
    COMPROMISO_FIRMADO = 15,
    ACTIVACION = 16,
    SUSPENSION = 17,
    REANUDACION = 18,
    PAUSA = 19,
    REACTIVACION = 20,
    INACTIVACION = 21,
    SALIDA_DE_INACTIVA = 22,
    RENOVACION_SOLICITADA = 23,
    RENOVACION_APROBADA = 24,
    RENOVACION_RECHAZADA = 25,
    CONCLUSION = 26,
    REVOCACION = 27,
    PAGO_REGISTRADO = 28,
    PAGO_CONFIRMADO = 29,
    PAGO_FALLIDO = 30,
    PAGO_DEVUELTO = 31,
    DATOS_PAGO_MODIFICADOS = 32,
    DATOS_PAGO_CONSULTADOS = 33,
    DOCUMENTO_RECHAZADO = 34,
    REPORTE_ACADEMICO = 35,
    CUMPLIMIENTO_EVALUADO = 36,
    CONSECUENCIA_APLICADA = 37,
    APORTE_REGISTRADO = 38
}

public static class TipoDecisionExtensions
{
    /// <summary>
    /// Tipos de decisión visibles para el Administrador Financiero en HU-35.2
    /// (sin exponer deliberaciones socioeconómicas ni evaluaciones del comité).
    /// </summary>
    public static IReadOnlySet<TipoDecision> AmbitoFinanciero { get; } = new HashSet<TipoDecision>
    {
        TipoDecision.PAGO_REGISTRADO,
        TipoDecision.PAGO_CONFIRMADO,
        TipoDecision.PAGO_FALLIDO,
        TipoDecision.PAGO_DEVUELTO,
        TipoDecision.DATOS_PAGO_MODIFICADOS,
        TipoDecision.APORTE_REGISTRADO
    };

    public static bool EsAmbitoFinanciero(this TipoDecision tipo) => AmbitoFinanciero.Contains(tipo);
}
