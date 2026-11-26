using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;

public sealed record RegistrarInicioInscripcionesCommand(
    ActorSolicitudCatalogo Actor,
    string CodigoPeriodo,
    Guid UniversidadId,
    string? NombreUniversidad,
    DateOnly? FechaInicio) : IRequest<InicioInscripcionesDto>;

public sealed class RegistrarInicioInscripcionesHandler
    : IRequestHandler<RegistrarInicioInscripcionesCommand, InicioInscripcionesDto>
{
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IInicioInscripcionesRepository _inicioRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarInicioInscripcionesHandler(
        ICalendarioAcademicoRepository calendarioRepo,
        IInicioInscripcionesRepository inicioRepo,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _calendarioRepo = calendarioRepo;
        _inicioRepo = inicioRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<InicioInscripcionesDto> Handle(
        RegistrarInicioInscripcionesCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionCalendario.ExigirDireccion(request.Actor);

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        if (calendario is null || !calendario.ExistePeriodo(codigo))
        {
            throw new ExcepcionDeDominio(
                $"El periodo {codigo.Valor} no está registrado.",
                "PERIODO_NO_ENCONTRADO");
        }

        // HU-38.1 Escenario 12: validar fecha obligatoria antes de consultar duplicado o crear.
        var nuevoInicio = InicioInscripcionesPeriodo.Crear(
            request.UniversidadId,
            request.NombreUniversidad,
            codigo,
            request.FechaInicio,
            _reloj.AhoraUtc);

        // HU-38.1 Escenario 13: una sola fecha de inicio de inscripciones por universidad y periodo.
        var existente = await _inicioRepo.ObtenerPorUniversidadYPeriodoAsync(
            request.UniversidadId,
            codigo,
            cancellationToken);

        if (existente is not null)
        {
            throw new ExcepcionDeDominio(
                $"La universidad {existente.NombreUniversidad} ya tiene registrada la fecha de inicio de inscripciones ({existente.FechaInicio:dd/MM/yyyy}) para el periodo {codigo.Valor}.",
                "INICIO_INSCRIPCIONES_DUPLICADO");
        }

        await _inicioRepo.AgregarAsync(nuevoInicio, cancellationToken);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return new InicioInscripcionesDto(
            nuevoInicio.Id,
            nuevoInicio.UniversidadId,
            nuevoInicio.NombreUniversidad,
            nuevoInicio.CodigoPeriodo.Valor,
            nuevoInicio.FechaInicio,
            nuevoInicio.InscripcionesIniciadas(_reloj));
    }
}
