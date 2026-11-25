using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S4.Becas.Domain.Becas;

/// <summary>
/// Esqueleto mínimo compartido del agregado Beca (DDD_v3.3 §11b, §15.2, ADR-16, ADR-25).
/// Las transiciones de ciclo de vida se incorporarán en los RF correspondientes sin acoplar RF-18.
/// </summary>
public sealed class Beca : RaizDeAgregado<Guid>
{
    private readonly List<TramoBeca> _tramos = [];

    public Guid PostulanteId { get; private set; }
    public Guid ProgramaId { get; private set; }
    public Guid OfertaId { get; private set; }
    public EstadoBeca Estado { get; private set; }
    public Alcance Alcance { get; private set; }
    public ModalidadPago Modalidad { get; private set; }
    public IReadOnlyCollection<TramoBeca> Tramos => _tramos.AsReadOnly();

    private Beca()
    {
    }

    public Beca(
        Guid id,
        Guid postulanteId,
        Guid programaId,
        Guid ofertaId,
        EstadoBeca estado,
        Alcance alcance,
        ModalidadPago modalidad,
        IEnumerable<TramoBeca>? tramos = null)
        : base(id)
    {
        if (id == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador de la beca es obligatorio.", "BECA_ID_INVALIDO");
        }

        if (postulanteId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador del postulante es obligatorio.", "POSTULANTE_ID_INVALIDO");
        }

        if (programaId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador del programa es obligatorio.", "PROGRAMA_ID_INVALIDO");
        }

        if (ofertaId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("El identificador de la oferta es obligatorio.", "OFERTA_ID_INVALIDO");
        }

        PostulanteId = postulanteId;
        ProgramaId = programaId;
        OfertaId = ofertaId;
        Estado = estado;
        Alcance = alcance;
        Modalidad = modalidad;

        if (tramos is not null)
        {
            _tramos.AddRange(tramos);
        }
    }
}
