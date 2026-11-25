namespace Ascenso.SharedKernel.Seguridad;

public enum CanalCodigo
{
    CORREO,
    SMS
}

/// <summary>
/// Operaciones sensibles que exigen OTP vinculado a usuario, operación, recurso y desafío (DDD-10, v1.3 §8.2, ADR-10).
/// </summary>
public enum TipoOperacionSensible
{
    CONFIRMAR_PAGO,
    RESOLVER_PAGO,
    MODIFICAR_DATOS_PAGO,
    REVOCAR_BECA,
    APLICAR_CONSECUENCIA_REVOCATORIA
}

public sealed record DesafioOtp(Guid ChallengeId, DateTimeOffset ExpiraEnUtc);

public interface IServicioOtp
{
    Task<DesafioOtp> SolicitarDesafioAsync(
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string recursoId,
        CanalCodigo canal,
        CancellationToken cancellationToken = default);

    Task<bool> ValidarYConsumirAsync(
        Guid usuarioId,
        TipoOperacionSensible operacion,
        string recursoId,
        Guid challengeId,
        string codigo,
        Guid operationId,
        CancellationToken cancellationToken = default);
}
