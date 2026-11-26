using Ascenso.Contracts.Eventos;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Universidades;

public sealed record RegistrarCostoMatriculaCommand(
    ActorSolicitudCatalogo Actor,
    Guid UniversidadId,
    string CodigoPeriodo,
    decimal Monto,
    string? Moneda) : IRequest<CostoMatriculaRegistroDto>;

public sealed class RegistrarCostoMatriculaHandler
    : IRequestHandler<RegistrarCostoMatriculaCommand, CostoMatriculaRegistroDto>
{
    public const string NombreEventoIntegracion = "CostoMatriculaVigente.v1";
    public const string MensajeSinPeriodos = "No hay periodos académicos registrados.";

    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IEscritorOutboxCatalogo _outbox;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarCostoMatriculaHandler(
        IUniversidadAliadaRepository universidadRepo,
        ICalendarioAcademicoRepository calendarioRepo,
        IEscritorOutboxCatalogo outbox,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _universidadRepo = universidadRepo;
        _calendarioRepo = calendarioRepo;
        _outbox = outbox;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<CostoMatriculaRegistroDto> Handle(
        RegistrarCostoMatriculaCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirGestionUniversidad(request.Actor);

        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        if (calendario is null || calendario.Periodos.Count == 0)
        {
            throw new ExcepcionDeDominio(MensajeSinPeriodos, "SIN_PERIODOS_REGISTRADOS");
        }

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        if (!calendario.ExistePeriodo(codigo))
        {
            throw new ExcepcionDeDominio($"El periodo {codigo.Valor} no está registrado.", "PERIODO_NO_ENCONTRADO");
        }

        var universidad = await _universidadRepo.ObtenerPorIdAsync(request.UniversidadId, cancellationToken)
            ?? throw new ExcepcionDeDominio("La universidad aliada indicada no está registrada.", "UNIVERSIDAD_NO_ENCONTRADA");

        var ahoraUtc = _reloj.AhoraUtc;
        var registro = universidad.RegistrarCosto(codigo, request.Monto, request.Moneda, ahoraUtc);

        var evento = new CostoMatriculaVigenteV1(
            universidad.Id,
            registro.Periodo.Valor,
            registro.Costo.Cantidad,
            registro.Costo.Moneda,
            registro.VersionRegistro,
            registro.RegistradoEnUtc);

        var correlationId = Guid.NewGuid();
        await _outbox.RegistrarEventoAsync(
            Guid.NewGuid(),
            NombreEventoIntegracion,
            schemaVersion: 1,
            evento,
            correlationId: correlationId,
            causationId: correlationId,
            occurredAtUtc: ahoraUtc,
            ct: cancellationToken);

        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return new CostoMatriculaRegistroDto(
            registro.Id,
            registro.Periodo.Valor,
            registro.Costo.Cantidad,
            registro.Costo.Moneda,
            registro.VersionRegistro,
            registro.RegistradoEnUtc);
    }
}

public sealed record RegistrarInicioInscripcionesUniversidadCommand(
    ActorSolicitudCatalogo Actor,
    Guid UniversidadId,
    string CodigoPeriodo,
    DateOnly? FechaInicio) : IRequest<InicioInscripcionesDto>;

public sealed class RegistrarInicioInscripcionesUniversidadHandler
    : IRequestHandler<RegistrarInicioInscripcionesUniversidadCommand, InicioInscripcionesDto>
{
    public const string MensajeFechaObligatoria = "Indica la fecha de inicio de inscripciones.";

    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IInicioInscripcionesRepository _inicioRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarInicioInscripcionesUniversidadHandler(
        IUniversidadAliadaRepository universidadRepo,
        ICalendarioAcademicoRepository calendarioRepo,
        IInicioInscripcionesRepository inicioRepo,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _universidadRepo = universidadRepo;
        _calendarioRepo = calendarioRepo;
        _inicioRepo = inicioRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<InicioInscripcionesDto> Handle(
        RegistrarInicioInscripcionesUniversidadCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirGestionUniversidad(request.Actor);

        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        if (calendario is null || calendario.Periodos.Count == 0)
        {
            throw new ExcepcionDeDominio(RegistrarCostoMatriculaHandler.MensajeSinPeriodos, "SIN_PERIODOS_REGISTRADOS");
        }

        if (request.FechaInicio is null)
        {
            throw new ExcepcionDeDominio(MensajeFechaObligatoria, "FECHA_INICIO_INSCRIPCIONES_OBLIGATORIA");
        }

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        if (!calendario.ExistePeriodo(codigo))
        {
            throw new ExcepcionDeDominio($"El periodo {codigo.Valor} no está registrado.", "PERIODO_NO_ENCONTRADO");
        }

        var universidad = await _universidadRepo.ObtenerPorIdAsync(request.UniversidadId, cancellationToken)
            ?? throw new ExcepcionDeDominio("La universidad aliada indicada no está registrada.", "UNIVERSIDAD_NO_ENCONTRADA");

        var existente = await _inicioRepo.ObtenerPorUniversidadYPeriodoAsync(universidad.Id, codigo, cancellationToken);
        if (existente is not null)
        {
            throw new ExcepcionDeDominio(
                $"La universidad {universidad.Nombre} ya tiene registrada la fecha de inicio de inscripciones ({existente.FechaInicio:dd/MM/yyyy}) para el periodo {codigo.Valor}.",
                "INICIO_INSCRIPCIONES_DUPLICADO");
        }

        var nuevo = InicioInscripcionesPeriodo.Crear(
            universidad.Id,
            universidad.Nombre,
            codigo,
            request.FechaInicio,
            _reloj.AhoraUtc);

        await _inicioRepo.AgregarAsync(nuevo, cancellationToken);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return new InicioInscripcionesDto(
            nuevo.Id,
            nuevo.UniversidadId,
            nuevo.NombreUniversidad,
            nuevo.CodigoPeriodo.Valor,
            nuevo.FechaInicio,
            nuevo.InscripcionesIniciadas(_reloj));
    }
}
