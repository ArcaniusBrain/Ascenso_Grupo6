namespace Ascenso.SharedKernel.Trazabilidad;

/// <summary>
/// Tipo de sujeto sobre el que recae una decisión registrada (DDD v3.3 §14).
/// </summary>
public enum TipoSujeto
{
    POSTULACION = 1,
    BECA = 2,
    POSTULANTE = 3,
    PROGRAMA = 4,
    DATOS_PAGO = 5
}
