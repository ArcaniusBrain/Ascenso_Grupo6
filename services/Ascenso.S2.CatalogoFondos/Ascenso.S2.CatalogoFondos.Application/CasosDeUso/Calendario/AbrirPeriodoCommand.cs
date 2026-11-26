using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;

public sealed record AbrirPeriodoCommand(
    ActorSolicitudCatalogo Actor,
    string? CodigoPeriodo) : IRequest<PeriodoResumenDto>;

public sealed class AbrirPeriodoHandler : IRequestHandler<AbrirPeriodoCommand, PeriodoResumenDto>
{
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public AbrirPeriodoHandler(
        ICalendarioAcademicoRepository calendarioRepo,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _calendarioRepo = calendarioRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<PeriodoResumenDto> Handle(AbrirPeriodoCommand request, CancellationToken cancellationToken)
    {
        PoliticaAutorizacionCalendario.ExigirDireccion(request.Actor);

        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        var esNuevo = calendario is null;
        calendario ??= CalendarioAcademico.CrearInstitucional();

        var periodo = calendario.AbrirPeriodo(request.CodigoPeriodo, _reloj.AhoraUtc);

        if (esNuevo)
        {
            await _calendarioRepo.AgregarAsync(calendario, cancellationToken);
        }

        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return new PeriodoResumenDto(
            periodo.Codigo.Valor,
            periodo.Estado.ToString(),
            periodo.EsActual,
            periodo.RegistradoEnUtc);
    }
}
