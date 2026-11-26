using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;

public sealed record ListarPeriodosQuery(ActorSolicitudCatalogo Actor) : IRequest<ListaPeriodosDto>;

public sealed class ListarPeriodosHandler : IRequestHandler<ListarPeriodosQuery, ListaPeriodosDto>
{
    public const string MensajeCalendarioVacio = "Aún no hay periodos académicos registrados.";

    private readonly ICalendarioAcademicoRepository _calendarioRepo;

    public ListarPeriodosHandler(ICalendarioAcademicoRepository calendarioRepo)
    {
        _calendarioRepo = calendarioRepo;
    }

    public async Task<ListaPeriodosDto> Handle(ListarPeriodosQuery request, CancellationToken cancellationToken)
    {
        PoliticaAutorizacionCalendario.ExigirPersonalInterno(request.Actor);

        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        if (calendario is null || calendario.Periodos.Count == 0)
        {
            return new ListaPeriodosDto(
                CalendarioAcademico.IdInstitucional,
                calendario?.Version ?? 0,
                null,
                MensajeCalendarioVacio,
                []);
        }

        var ordenados = calendario.ObtenerPeriodosOrdenados()
            .Select(p => new PeriodoResumenDto(
                p.Codigo.Valor,
                p.Estado.ToString(),
                p.EsActual,
                p.RegistradoEnUtc))
            .ToArray();

        return new ListaPeriodosDto(
            calendario.Id,
            calendario.Version,
            calendario.PeriodoActual()?.Codigo.Valor,
            null,
            ordenados);
    }
}

public sealed record ConsultarPeriodoDetalleQuery(
    ActorSolicitudCatalogo Actor,
    string CodigoPeriodo) : IRequest<PeriodoDetalleDto>;

public sealed class ConsultarPeriodoDetalleHandler
    : IRequestHandler<ConsultarPeriodoDetalleQuery, PeriodoDetalleDto>
{
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IInicioInscripcionesRepository _inicioRepo;
    private readonly IConsultorElementosPeriodo _consultorElementos;
    private readonly IRelojNegocio _reloj;

    public ConsultarPeriodoDetalleHandler(
        ICalendarioAcademicoRepository calendarioRepo,
        IInicioInscripcionesRepository inicioRepo,
        IConsultorElementosPeriodo consultorElementos,
        IRelojNegocio reloj)
    {
        _calendarioRepo = calendarioRepo;
        _inicioRepo = inicioRepo;
        _consultorElementos = consultorElementos;
        _reloj = reloj;
    }

    public async Task<PeriodoDetalleDto> Handle(ConsultarPeriodoDetalleQuery request, CancellationToken cancellationToken)
    {
        PoliticaAutorizacionCalendario.ExigirPersonalInterno(request.Actor);

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        var periodo = calendario?.ObtenerPeriodo(codigo);

        if (periodo is null)
        {
            throw new ExcepcionDeDominio(
                $"El periodo {codigo.Valor} no está registrado.",
                "PERIODO_NO_ENCONTRADO");
        }

        var periodoActual = calendario!.PeriodoActual();
        var esPeriodoAnterior = !periodo.EsActual
            && periodoActual is not null
            && ComparadorCodigoPeriodo.Instancia.Compare(periodo.Codigo, periodoActual.Codigo) < 0;

        var inicios = await _inicioRepo.ListarPorPeriodoAsync(codigo, cancellationToken);
        var iniciosDto = inicios
            .OrderBy(i => i.FechaInicio)
            .ThenBy(i => i.NombreUniversidad, StringComparer.OrdinalIgnoreCase)
            .Select(i => new InicioInscripcionesDto(
                i.Id,
                i.UniversidadId,
                i.NombreUniversidad,
                i.CodigoPeriodo.Valor,
                i.FechaInicio,
                i.InscripcionesIniciadas(_reloj)))
            .ToArray();

        var cantidadElementos = await _consultorElementos.ContarElementosLigadosAsync(codigo, cancellationToken);
        var mensajeSinElementos = cantidadElementos == 0
            ? $"El periodo {codigo.Valor} no tiene programas, ciclos ni renovaciones."
            : null;

        return new PeriodoDetalleDto(
            periodo.Codigo.Valor,
            periodo.Estado.ToString(),
            periodo.EsActual,
            esPeriodoAnterior,
            periodo.RegistradoEnUtc,
            cantidadElementos,
            mensajeSinElementos,
            iniciosDto);
    }
}
