using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;

public sealed record CerrarPeriodoCommand(
    ActorSolicitudCatalogo Actor,
    string CodigoPeriodo) : IRequest<PeriodoResumenDto>;

public sealed class CerrarPeriodoHandler : IRequestHandler<CerrarPeriodoCommand, PeriodoResumenDto>
{
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;

    public CerrarPeriodoHandler(
        ICalendarioAcademicoRepository calendarioRepo,
        IUnidadTrabajoCatalogo unidadTrabajo)
    {
        _calendarioRepo = calendarioRepo;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<PeriodoResumenDto> Handle(CerrarPeriodoCommand request, CancellationToken cancellationToken)
    {
        PoliticaAutorizacionCalendario.ExigirDireccion(request.Actor);

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken)
            ?? throw new ExcepcionDeDominio($"El periodo {codigo.Valor} no está registrado.", "PERIODO_NO_ENCONTRADO");

        var periodo = calendario.CerrarPeriodo(codigo);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return new PeriodoResumenDto(
            periodo.Codigo.Valor,
            periodo.Estado.ToString(),
            periodo.EsActual,
            periodo.RegistradoEnUtc);
    }
}
