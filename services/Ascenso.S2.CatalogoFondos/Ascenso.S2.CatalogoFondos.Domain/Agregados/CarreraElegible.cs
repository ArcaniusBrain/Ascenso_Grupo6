using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Entidad interna de UniversidadAliada que representa una carrera elegible propia de esa universidad (HU-14.1 E4).
/// </summary>
public sealed class CarreraElegible : Entidad<Guid>
{
    public Guid UniversidadAliadaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string NombreNormalizado { get; private set; } = string.Empty;

    private CarreraElegible()
    {
    }

    internal CarreraElegible(
        Guid id,
        Guid universidadAliadaId,
        string nombre,
        string nombreNormalizado) : base(id)
    {
        UniversidadAliadaId = universidadAliadaId;
        Nombre = nombre;
        NombreNormalizado = nombreNormalizado;
    }
}
