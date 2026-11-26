using Ascenso.S6.ComunicacionesReporteria.Application.Puertos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;
using Ascenso.S6.ComunicacionesReporteria.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using MediatR;

namespace Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.Consentimientos;

public sealed record ConsentimientoDonanteDto(
    Guid Id,
    Guid BecarioId,
    Guid DonanteId,
    DateTimeOffset AceptadoEnUtc,
    bool Irrevocable);

public sealed record ConsentimientoRegistroResultadoDto(
    bool Registrado,
    bool YaExistia,
    ConsentimientoDonanteDto? Consentimiento,
    string? Mensaje);

public sealed record RegistrarConsentimientoDonanteCommand(
    ActorSolicitudComunicaciones Actor,
    Guid DonanteId,
    bool AceptaPresentacionIndividual,
    Guid? BecarioObjetivoId = null) : IRequest<ConsentimientoRegistroResultadoDto>;

public sealed class RegistrarConsentimientoDonanteHandler
    : IRequestHandler<RegistrarConsentimientoDonanteCommand, ConsentimientoRegistroResultadoDto>
{
    private readonly IConsentimientoDonanteRepository _consentimientoRepo;
    private readonly IUnidadTrabajoComunicaciones _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarConsentimientoDonanteHandler(
        IConsentimientoDonanteRepository consentimientoRepo,
        IUnidadTrabajoComunicaciones unidadTrabajo,
        IRelojNegocio reloj)
    {
        _consentimientoRepo = consentimientoRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<ConsentimientoRegistroResultadoDto> Handle(
        RegistrarConsentimientoDonanteCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionImpacto.ExigirPropioBecario(request.Actor, request.BecarioObjetivoId);

        if (!request.AceptaPresentacionIndividual)
        {
            return new ConsentimientoRegistroResultadoDto(
                Registrado: false,
                YaExistia: false,
                Consentimiento: null,
                Mensaje: "No se registró consentimiento; solo aparecerás en datos agregados.");
        }

        var existente = await _consentimientoRepo.ObtenerPorBecarioYDonanteAsync(
            request.Actor.UsuarioId,
            request.DonanteId,
            cancellationToken);

        // HU-33.2 E4: confirmar dos veces conserva el único registro vigente con su fecha original sin duplicar.
        if (existente is not null)
        {
            return new ConsentimientoRegistroResultadoDto(
                Registrado: true,
                YaExistia: true,
                Consentimiento: Mapear(existente),
                Mensaje: "Tu consentimiento ya estaba registrado y permanece vigente.");
        }

        var nuevo = ConsentimientoDonante.Aceptar(request.Actor.UsuarioId, request.DonanteId, _reloj.AhoraUtc);
        await _consentimientoRepo.AgregarAsync(nuevo, cancellationToken);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return new ConsentimientoRegistroResultadoDto(
            Registrado: true,
            YaExistia: false,
            Consentimiento: Mapear(nuevo),
            Mensaje: null);
    }

    internal static ConsentimientoDonanteDto Mapear(ConsentimientoDonante c) =>
        new(c.Id, c.BecarioId, c.DonanteId, c.AceptadoEnUtc, Irrevocable: true);
}

public sealed record ListarMisConsentimientosQuery(ActorSolicitudComunicaciones Actor)
    : IRequest<IReadOnlyList<ConsentimientoDonanteDto>>;

public sealed class ListarMisConsentimientosHandler
    : IRequestHandler<ListarMisConsentimientosQuery, IReadOnlyList<ConsentimientoDonanteDto>>
{
    private readonly IConsentimientoDonanteRepository _consentimientoRepo;

    public ListarMisConsentimientosHandler(IConsentimientoDonanteRepository consentimientoRepo) =>
        _consentimientoRepo = consentimientoRepo;

    public async Task<IReadOnlyList<ConsentimientoDonanteDto>> Handle(
        ListarMisConsentimientosQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionImpacto.ExigirPropioBecario(request.Actor);

        var lista = await _consentimientoRepo.ListarPorBecarioAsync(request.Actor.UsuarioId, cancellationToken);
        return lista
            .OrderByDescending(c => c.AceptadoEnUtc)
            .Select(RegistrarConsentimientoDonanteHandler.Mapear)
            .ToArray();
    }
}
