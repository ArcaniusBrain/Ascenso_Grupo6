namespace Ascenso.SharedKernel.TiposBase;

public abstract class Entidad<TId> : IEquatable<Entidad<TId>>
    where TId : notnull
{
    private readonly List<IEventoDeDominio> _eventosPendientes = [];

    public TId Id { get; protected init; } = default!;

    public IReadOnlyCollection<IEventoDeDominio> EventosPendientes => _eventosPendientes.AsReadOnly();

    protected Entidad()
    {
    }

    protected Entidad(TId id)
    {
        if (id is null)
        {
            throw new ExcepcionDeDominio("El identificador de la entidad es obligatorio.", "ID_OBLIGATORIO");
        }

        Id = id;
    }

    protected void Registrar(IEventoDeDominio evento)
    {
        ArgumentNullException.ThrowIfNull(evento);
        _eventosPendientes.Add(evento);
    }

    public void LimpiarEventos() => _eventosPendientes.Clear();

    public bool Equals(Entidad<TId>? otro)
    {
        if (otro is null) return false;
        if (ReferenceEquals(this, otro)) return true;
        if (GetType() != otro.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, otro.Id);
    }

    public override bool Equals(object? obj) => obj is Entidad<TId> otra && Equals(otra);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entidad<TId>? izquierda, Entidad<TId>? derecha)
        => Equals(izquierda, derecha);

    public static bool operator !=(Entidad<TId>? izquierda, Entidad<TId>? derecha)
        => !Equals(izquierda, derecha);
}
