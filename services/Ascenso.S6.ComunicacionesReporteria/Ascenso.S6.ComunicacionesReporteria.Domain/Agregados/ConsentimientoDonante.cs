using Ascenso.SharedKernel.TiposBase;

namespace Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;

public sealed record ConsentimientoDonanteRegistradoEvento(
    Guid ConsentimientoId,
    Guid BecarioId,
    Guid DonanteId,
    DateTimeOffset Fecha) : IEventoDeDominio;

/// <summary>
/// Raíz de agregado ConsentimientoDonante (RF-33 · HU-33.2, DDD v3.3 §14, R36/A6, ADR-25).
/// Un registro único por (BecarioId, DonanteId), otorgado exclusivamente por el propio becario
/// e irrevocable una vez aceptado (sin operación de retiro).
/// </summary>
public sealed class ConsentimientoDonante : RaizDeAgregado<Guid>
{
    public Guid BecarioId { get; private set; }
    public Guid DonanteId { get; private set; }
    public DateTimeOffset AceptadoEnUtc { get; private set; }

    private ConsentimientoDonante()
    {
    }

    private ConsentimientoDonante(
        Guid id,
        Guid becarioId,
        Guid donanteId,
        DateTimeOffset aceptadoEnUtc) : base(id)
    {
        BecarioId = becarioId;
        DonanteId = donanteId;
        AceptadoEnUtc = aceptadoEnUtc;
    }

    public static ConsentimientoDonante Aceptar(
        Guid becarioId,
        Guid donanteId,
        DateTimeOffset ahoraUtc)
    {
        if (becarioId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica el becario que otorga el consentimiento.", "BECARIO_INVALIDO");
        }

        if (donanteId == Guid.Empty)
        {
            throw new ExcepcionDeDominio("Indica el donante para el cual se otorga el consentimiento.", "DONANTE_INVALIDO");
        }

        var consentimiento = new ConsentimientoDonante(Guid.NewGuid(), becarioId, donanteId, ahoraUtc);
        consentimiento.Registrar(new ConsentimientoDonanteRegistradoEvento(
            consentimiento.Id,
            becarioId,
            donanteId,
            ahoraUtc));

        return consentimiento;
    }
}
