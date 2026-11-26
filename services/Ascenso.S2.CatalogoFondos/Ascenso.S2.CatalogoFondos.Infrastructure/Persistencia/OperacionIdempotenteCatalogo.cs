namespace Ascenso.S2.CatalogoFondos.Infrastructure.Persistencia;

/// <summary>
/// Registro persistido para idempotencia de comandos mutadores en S2 por operationId (ADR-13).
/// </summary>
public sealed class OperacionIdempotenteCatalogo
{
    public Guid OperationId { get; private set; }
    public Guid ResultadoId { get; private set; }
    public string Estado { get; private set; } = string.Empty;
    public Guid CorrelationId { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset ProcesadoEnUtc { get; private set; }

    private OperacionIdempotenteCatalogo()
    {
    }

    public static OperacionIdempotenteCatalogo Crear(
        Guid operationId,
        Guid resultadoId,
        string estado,
        Guid correlationId,
        int version,
        DateTimeOffset procesadoEnUtc) =>
        new()
        {
            OperationId = operationId,
            ResultadoId = resultadoId,
            Estado = estado,
            CorrelationId = correlationId,
            Version = version,
            ProcesadoEnUtc = procesadoEnUtc
        };
}
