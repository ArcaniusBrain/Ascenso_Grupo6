using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.Trazabilidad;

/// <summary>
/// Se lanza ante cualquier intento de editar o eliminar un RegistroDecision ya persistido (RF-35, HU-35.1).
/// </summary>
public sealed class ExcepcionInmutabilidadRegistro : ExcepcionDeDominio
{
    public const string MensajeEstandar = "El registro de decisiones no se puede modificar ni eliminar.";

    public ExcepcionInmutabilidadRegistro()
        : base(MensajeEstandar, "REGISTRO_DECISION_INMUTABLE")
    {
    }
}

/// <summary>
/// Se lanza cuando un actor sin autorización o fuera de alcance intenta registrar o consultar decisiones (RF-35, RF-37).
/// </summary>
public sealed class ExcepcionAccesoDenegadoDecision : ExcepcionDeDominio
{
    public ExcepcionAccesoDenegadoDecision(string mensaje)
        : base(mensaje, "REGISTRO_DECISION_ACCESO_DENEGADO")
    {
    }
}
