using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S2.CatalogoFondos.Domain.Agregados;

/// <summary>
/// Relación estructurada programa–universidad–carrera (HU-11.2, Anexo A).
/// Cuando CarreraNombre es nulo, representa restricción a nivel de universidad completa.
/// </summary>
public sealed class RestriccionElegibilidad : Entidad<Guid>
{
    public Guid ProgramaId { get; private set; }
    public Guid UniversidadId { get; private set; }
    public string NombreUniversidad { get; private set; } = string.Empty;
    public string? CarreraNombre { get; private set; }

    private RestriccionElegibilidad()
    {
    }

    internal RestriccionElegibilidad(
        Guid id,
        Guid programaId,
        Guid universidadId,
        string nombreUniversidad,
        string? carreraNombre) : base(id)
    {
        ProgramaId = programaId;
        UniversidadId = universidadId;
        NombreUniversidad = nombreUniversidad;
        CarreraNombre = string.IsNullOrWhiteSpace(carreraNombre) ? null : carreraNombre.Trim();
    }
}

/// <summary>
/// Asociación única entre un programa de becas y un donante (HU-11.2 E10).
/// </summary>
public sealed class AsociacionDonantePrograma : Entidad<Guid>
{
    public Guid ProgramaId { get; private set; }
    public Guid DonanteId { get; private set; }
    public DateTimeOffset AsociadoEnUtc { get; private set; }

    private AsociacionDonantePrograma()
    {
    }

    internal AsociacionDonantePrograma(
        Guid id,
        Guid programaId,
        Guid donanteId,
        DateTimeOffset asociadoEnUtc) : base(id)
    {
        ProgramaId = programaId;
        DonanteId = donanteId;
        AsociadoEnUtc = asociadoEnUtc;
    }
}
