using Ascenso.S4.Becas.Domain.Becas;

namespace Ascenso.S4.Becas.Infrastructure.Persistencia;

/// <summary>
/// Proyección local mínima de personas en el schema s4 (sin datos socioeconómicos, HU-18.1).
/// </summary>
public sealed class ReferenciaPersona
{
    public Guid PersonaId { get; private set; }
    public string Cedula { get; private set; } = string.Empty;
    public string NombreCompleto { get; private set; } = string.Empty;

    private ReferenciaPersona()
    {
    }

    public ReferenciaPersona(Guid personaId, string cedula, string nombreCompleto)
    {
        PersonaId = personaId;
        Cedula = cedula;
        NombreCompleto = nombreCompleto;
    }
}

/// <summary>
/// Proyección local mínima de programas en el schema s4 (sin leer schema s2).
/// </summary>
public sealed class ReferenciaPrograma
{
    public Guid ProgramaId { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public Alcance Alcance { get; private set; }

    private ReferenciaPrograma()
    {
    }

    public ReferenciaPrograma(Guid programaId, string nombre, Alcance alcance)
    {
        ProgramaId = programaId;
        Nombre = nombre;
        Alcance = alcance;
    }
}
