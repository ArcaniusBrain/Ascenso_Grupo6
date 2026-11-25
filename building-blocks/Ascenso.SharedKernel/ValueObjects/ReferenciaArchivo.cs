using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.SharedKernel.ValueObjects;

/// <summary>
/// Referencia privada de archivo almacenado en Firebase Storage (ADR-06, DDD §5).
/// No admite URLs públicas persistentes.
/// </summary>
public sealed record ReferenciaArchivo
{
    public string RutaFirebase { get; }

    public ReferenciaArchivo(string rutaFirebase)
    {
        var limpia = (rutaFirebase ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(limpia))
        {
            throw new ExcepcionDeDominio("La ruta interna de Firebase Storage es obligatoria.", "REFERENCIA_ARCHIVO_VACIA");
        }

        if (limpia.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || limpia.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            throw new ExcepcionDeDominio(
                "ReferenciaArchivo solo admite rutas internas privadas de Firebase Storage, nunca URLs públicas.",
                "REFERENCIA_ARCHIVO_URL_PUBLICA");
        }

        RutaFirebase = limpia;
    }

    public override string ToString() => RutaFirebase;
}
