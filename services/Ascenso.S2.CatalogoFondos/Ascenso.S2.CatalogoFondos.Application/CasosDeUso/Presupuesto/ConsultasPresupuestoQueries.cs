using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.TiposBase;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Presupuesto;

public sealed record ConsultarPresupuestoProgramaQuery(
    ActorSolicitudCatalogo Actor,
    Guid ProgramaId) : IRequest<PresupuestoProgramaDto>;

public sealed class ConsultarPresupuestoProgramaHandler
    : IRequestHandler<ConsultarPresupuestoProgramaQuery, PresupuestoProgramaDto>
{
    private readonly IProgramaRepository _programaRepo;
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;

    public ConsultarPresupuestoProgramaHandler(
        IProgramaRepository programaRepo,
        IPresupuestoProgramaRepository presupuestoRepo)
    {
        _programaRepo = programaRepo;
        _presupuestoRepo = presupuestoRepo;
    }

    public async Task<PresupuestoProgramaDto> Handle(
        ConsultarPresupuestoProgramaQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionPresupuesto.ExigirConsultaPresupuesto(request.Actor);

        var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

        var presupuesto = await _presupuestoRepo.ObtenerPorProgramaIdAsync(programa.Id, cancellationToken);
        if (presupuesto is null)
        {
            return new PresupuestoProgramaDto(
                programa.Id,
                programa.Nombre,
                programa.MontoPorPeriodo.Moneda,
                PresupuestoTotal: 0m,
                TotalOtorgado: 0m,
                ComprometidoOfertasPendientes: 0m,
                ComprometidoRenovacionesPendientes: 0m,
                PresupuestoDisponible: 0m,
                Desembolsado: 0m,
                CupoTotal: programa.CupoBecarios,
                CupoOcupado: 0,
                CupoDisponible: programa.CupoBecarios,
                MensajeSinAportes: PresupuestoPrograma.MensajeSinAportes,
                Version: 0);
        }

        return new PresupuestoProgramaDto(
            programa.Id,
            programa.Nombre,
            presupuesto.Moneda,
            presupuesto.Aportes,
            presupuesto.Otorgado,
            presupuesto.OfertasPendientes,
            presupuesto.RenovacionesPendientes,
            presupuesto.MontoDisponible,
            presupuesto.Desembolsado,
            presupuesto.CupoBecarios,
            presupuesto.CuposOcupados,
            presupuesto.CupoDisponible,
            presupuesto.Aportes == 0m ? PresupuestoPrograma.MensajeSinAportes : null,
            presupuesto.Version);
    }
}

public sealed record ConsultarDisponibilidadInternaQuery(Guid ProgramaId) : IRequest<DisponibilidadInternaDto>;

public sealed class ConsultarDisponibilidadInternaHandler
    : IRequestHandler<ConsultarDisponibilidadInternaQuery, DisponibilidadInternaDto>
{
    private readonly IProgramaRepository _programaRepo;
    private readonly IPresupuestoProgramaRepository _presupuestoRepo;

    public ConsultarDisponibilidadInternaHandler(
        IProgramaRepository programaRepo,
        IPresupuestoProgramaRepository presupuestoRepo)
    {
        _programaRepo = programaRepo;
        _presupuestoRepo = presupuestoRepo;
    }

    public async Task<DisponibilidadInternaDto> Handle(
        ConsultarDisponibilidadInternaQuery request,
        CancellationToken cancellationToken)
    {
        var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

        var presupuesto = await _presupuestoRepo.ObtenerPorProgramaIdAsync(programa.Id, cancellationToken);
        if (presupuesto is null)
        {
            return new DisponibilidadInternaDto(0m, programa.CupoBecarios);
        }

        return new DisponibilidadInternaDto(presupuesto.MontoDisponible, presupuesto.CupoDisponible);
    }
}
