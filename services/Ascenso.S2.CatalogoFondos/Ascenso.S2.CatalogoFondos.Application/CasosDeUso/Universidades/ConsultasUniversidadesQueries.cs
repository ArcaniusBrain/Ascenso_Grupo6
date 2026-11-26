using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Universidades;

public sealed record ListarUniversidadesQuery(ActorSolicitudCatalogo Actor)
    : IRequest<IReadOnlyList<UniversidadResumenDto>>;

public sealed class ListarUniversidadesHandler
    : IRequestHandler<ListarUniversidadesQuery, IReadOnlyList<UniversidadResumenDto>>
{
    private readonly IUniversidadAliadaRepository _universidadRepo;

    public ListarUniversidadesHandler(IUniversidadAliadaRepository universidadRepo) =>
        _universidadRepo = universidadRepo;

    public async Task<IReadOnlyList<UniversidadResumenDto>> Handle(
        ListarUniversidadesQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirUsuarioAutenticado(request.Actor);

        var todas = await _universidadRepo.ListarTodasAsync(cancellationToken);
        return todas
            .OrderBy(u => u.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(RegistrarUniversidadAliadaHandler.MapearResumen)
            .ToArray();
    }
}

public sealed record ConsultarUniversidadDetalleQuery(
    ActorSolicitudCatalogo Actor,
    Guid UniversidadId) : IRequest<UniversidadDetalleDto>;

public sealed class ConsultarUniversidadDetalleHandler
    : IRequestHandler<ConsultarUniversidadDetalleQuery, UniversidadDetalleDto>
{
    private readonly IUniversidadAliadaRepository _universidadRepo;

    public ConsultarUniversidadDetalleHandler(IUniversidadAliadaRepository universidadRepo) =>
        _universidadRepo = universidadRepo;

    public async Task<UniversidadDetalleDto> Handle(
        ConsultarUniversidadDetalleQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirUsuarioAutenticado(request.Actor);

        var universidad = await _universidadRepo.ObtenerPorIdAsync(request.UniversidadId, cancellationToken)
            ?? throw new ExcepcionDeDominio("La universidad aliada indicada no está registrada.", "UNIVERSIDAD_NO_ENCONTRADA");

        var carreras = universidad.Carreras
            .OrderBy(c => c.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(c => new CarreraElegibleDto(c.Id, c.Nombre))
            .ToArray();

        var costosVigentes = universidad.CostosVigentesPorPeriodo()
            .Select(c => new CostoMatriculaRegistroDto(
                c.Id,
                c.Periodo.Valor,
                c.Costo.Cantidad,
                c.Costo.Moneda,
                c.VersionRegistro,
                c.RegistradoEnUtc))
            .ToArray();

        return new UniversidadDetalleDto(
            universidad.Id,
            universidad.Nombre,
            universidad.Convenio.Terminos,
            universidad.Convenio.Via.ToString(),
            new ContactoDesignadoDto(
                universidad.ContactoDesignado.NombreCompleto,
                universidad.ContactoDesignado.Correo,
                universidad.ContactoDesignado.Telefono),
            universidad.Version,
            carreras.Length == 0 ? UniversidadAliada.MensajeSinCarreras : null,
            carreras,
            costosVigentes);
    }
}

public sealed record ConsultarCostoMatriculaPeriodoQuery(
    ActorSolicitudCatalogo Actor,
    Guid UniversidadId,
    string CodigoPeriodo) : IRequest<CostoMatriculaPeriodoDto>;

public sealed class ConsultarCostoMatriculaPeriodoHandler
    : IRequestHandler<ConsultarCostoMatriculaPeriodoQuery, CostoMatriculaPeriodoDto>
{
    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly ICalendarioAcademicoRepository _calendarioRepo;

    public ConsultarCostoMatriculaPeriodoHandler(
        IUniversidadAliadaRepository universidadRepo,
        ICalendarioAcademicoRepository calendarioRepo)
    {
        _universidadRepo = universidadRepo;
        _calendarioRepo = calendarioRepo;
    }

    public async Task<CostoMatriculaPeriodoDto> Handle(
        ConsultarCostoMatriculaPeriodoQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirUsuarioAutenticado(request.Actor);

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        if (calendario is null || !calendario.ExistePeriodo(codigo))
        {
            throw new ExcepcionDeDominio($"El periodo {codigo.Valor} no está registrado.", "PERIODO_NO_ENCONTRADO");
        }

        var universidad = await _universidadRepo.ObtenerPorIdAsync(request.UniversidadId, cancellationToken)
            ?? throw new ExcepcionDeDominio("La universidad aliada indicada no está registrada.", "UNIVERSIDAD_NO_ENCONTRADA");

        var historial = universidad.HistorialCostos(codigo)
            .Select(c => new CostoMatriculaRegistroDto(
                c.Id,
                c.Periodo.Valor,
                c.Costo.Cantidad,
                c.Costo.Moneda,
                c.VersionRegistro,
                c.RegistradoEnUtc))
            .ToArray();

        var vigente = historial.LastOrDefault();

        return new CostoMatriculaPeriodoDto(
            universidad.Id,
            codigo.Valor,
            vigente,
            vigente is null ? UniversidadAliada.MensajeSinCostoEnPeriodo : null,
            historial);
    }
}

public sealed record ConsultarInicioInscripcionesUniversidadQuery(
    ActorSolicitudCatalogo Actor,
    Guid UniversidadId,
    string CodigoPeriodo) : IRequest<InicioInscripcionesUniversidadDto>;

public sealed class ConsultarInicioInscripcionesUniversidadHandler
    : IRequestHandler<ConsultarInicioInscripcionesUniversidadQuery, InicioInscripcionesUniversidadDto>
{
    public const string MensajeSinFechaEnPeriodo = "Sin fecha de inicio de inscripciones registrada para este periodo";

    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IInicioInscripcionesRepository _inicioRepo;
    private readonly IRelojNegocio _reloj;

    public ConsultarInicioInscripcionesUniversidadHandler(
        IUniversidadAliadaRepository universidadRepo,
        ICalendarioAcademicoRepository calendarioRepo,
        IInicioInscripcionesRepository inicioRepo,
        IRelojNegocio reloj)
    {
        _universidadRepo = universidadRepo;
        _calendarioRepo = calendarioRepo;
        _inicioRepo = inicioRepo;
        _reloj = reloj;
    }

    public async Task<InicioInscripcionesUniversidadDto> Handle(
        ConsultarInicioInscripcionesUniversidadQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionUniversidades.ExigirUsuarioAutenticado(request.Actor);

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        if (calendario is null || !calendario.ExistePeriodo(codigo))
        {
            throw new ExcepcionDeDominio($"El periodo {codigo.Valor} no está registrado.", "PERIODO_NO_ENCONTRADO");
        }

        var universidad = await _universidadRepo.ObtenerPorIdAsync(request.UniversidadId, cancellationToken)
            ?? throw new ExcepcionDeDominio("La universidad aliada indicada no está registrada.", "UNIVERSIDAD_NO_ENCONTRADA");

        var inicio = await _inicioRepo.ObtenerPorUniversidadYPeriodoAsync(universidad.Id, codigo, cancellationToken);
        if (inicio is null)
        {
            return new InicioInscripcionesUniversidadDto(
                universidad.Id,
                codigo.Valor,
                null,
                false,
                MensajeSinFechaEnPeriodo);
        }

        return new InicioInscripcionesUniversidadDto(
            universidad.Id,
            codigo.Valor,
            inicio.FechaInicio,
            inicio.InscripcionesIniciadas(_reloj),
            null);
    }
}
