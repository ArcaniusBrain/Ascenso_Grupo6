namespace Ascenso.SharedKernel.TiposBase;

/// <summary>
/// Clase base para toda raíz de agregado del modelo DDD (sección 5).
/// Concurrencia optimista explícita (DDD-07, ADR-16, ADR-25): toda operación que modifique
/// el agregado —incluso cuando solo cambie una entidad interna— debe invocar IncrementarVersion().
/// </summary>
public abstract class RaizDeAgregado<TId> : Entidad<TId>
    where TId : notnull
{
    public int Version { get; protected set; }

    protected RaizDeAgregado()
    {
    }

    protected RaizDeAgregado(TId id) : base(id)
    {
        Version = 0;
    }

    protected void IncrementarVersion()
    {
        Version++;
    }
}
