using System.Text.Json;
using Ascenso.S6.ComunicacionesReporteria.Application.Puertos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Agregados;
using Ascenso.S6.ComunicacionesReporteria.Domain.Modelos;
using Ascenso.S6.ComunicacionesReporteria.Domain.Repositorios;
using Ascenso.S6.ComunicacionesReporteria.Domain.Servicios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.ReportesImpacto;

public sealed record SolicitudReporteImpactoDto(
    Guid ReporteId,
    Guid DonanteId,
    string NombreDonante,
    string CodigoPeriodo,
    string Estado,
    string? MensajeEstado,
    DateTimeOffset SolicitadoEnUtc,
    DateTimeOffset? CompletadoEnUtc,
    ReporteImpactoContenido? Contenido);

public sealed record SolicitarReporteImpactoCommand(
    ActorSolicitudComunicaciones Actor,
    Guid DonanteId,
    string? CodigoPeriodo) : IRequest<SolicitudReporteImpactoDto>;

public sealed class SolicitarReporteImpactoHandler
    : IRequestHandler<SolicitarReporteImpactoCommand, SolicitudReporteImpactoDto>
{
    private readonly IRepositorioProyeccionImpacto _proyecciones;
    private readonly ISolicitudReporteImpactoRepository _solicitudRepo;
    private readonly IColaProcesamientoReportesImpacto _cola;
    private readonly IUnidadTrabajoComunicaciones _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public SolicitarReporteImpactoHandler(
        IRepositorioProyeccionImpacto proyecciones,
        ISolicitudReporteImpactoRepository solicitudRepo,
        IColaProcesamientoReportesImpacto cola,
        IUnidadTrabajoComunicaciones unidadTrabajo,
        IRelojNegocio reloj)
    {
        _proyecciones = proyecciones;
        _solicitudRepo = solicitudRepo;
        _cola = cola;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<SolicitudReporteImpactoDto> Handle(
        SolicitarReporteImpactoCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionImpacto.ExigirGeneracionReporte(request.Actor);

        if (string.IsNullOrWhiteSpace(request.CodigoPeriodo))
        {
            throw new ExcepcionDeDominio("Indica el periodo del reporte.", "PERIODO_OBLIGATORIO");
        }

        var periodo = new CodigoPeriodo(request.CodigoPeriodo);
        if (!await _proyecciones.ExistePeriodoEnCalendarioAsync(periodo.Valor, cancellationToken))
        {
            throw new ExcepcionDeDominio(
                SolicitudReporteImpacto.MensajePeriodoInexistente(periodo.Valor),
                "PERIODO_NO_ENCONTRADO");
        }

        var aportes = await _proyecciones.ObtenerAportesDonanteEnPeriodoAsync(
            request.DonanteId,
            periodo.Valor,
            cancellationToken);

        if (aportes.Count == 0)
        {
            throw new ExcepcionDeDominio(
                SolicitudReporteImpacto.MensajeSinAportesEnPeriodo(periodo.Valor),
                "DONANTE_SIN_APORTES_EN_PERIODO");
        }

        IReadOnlyCollection<Guid>? programasAlcance = null;
        if (string.Equals(request.Actor.Rol.Trim(), "COORDINADOR", StringComparison.OrdinalIgnoreCase)
            && request.Actor.ProgramasAsignados is { Count: > 0 } asignados)
        {
            var permitidos = aportes.Where(a => asignados.Contains(a.ProgramaId)).ToArray();
            if (permitidos.Length == 0)
            {
                throw new ExcepcionDeDominio(
                    "Solo puedes generar reportes de impacto sobre los programas que coordinas.",
                    "ACCESO_DENEGADO_REPORTES");
            }

            programasAlcance = permitidos.Select(a => a.ProgramaId).Distinct().ToArray();
        }

        var nombreDonante = aportes[0].NombreDonante;
        var solicitud = SolicitudReporteImpacto.Crear(
            request.DonanteId,
            nombreDonante,
            periodo,
            request.Actor.UsuarioId,
            request.Actor.Rol,
            programasAlcance,
            _reloj.AhoraUtc);

        await _solicitudRepo.AgregarAsync(solicitud, cancellationToken);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        // Encolar generación asíncrona en segundo plano (HU-33.1 E9, E12) sin bloquear el request HTTP.
        await _cola.EncolarAsync(solicitud.Id, cancellationToken);

        return MapearDto(solicitud);
    }

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static SolicitudReporteImpactoDto MapearDto(SolicitudReporteImpacto s)
    {
        ReporteImpactoContenido? contenido = null;
        if (s.Estado == EstadoSolicitudReporteImpacto.LISTO && !string.IsNullOrWhiteSpace(s.ContenidoJson))
        {
            contenido = JsonSerializer.Deserialize<ReporteImpactoContenido>(s.ContenidoJson, JsonOptions);
        }

        return new SolicitudReporteImpactoDto(
            s.Id,
            s.DonanteId,
            s.NombreDonante,
            s.Periodo.Valor,
            s.Estado.ToString(),
            s.MensajeEstado,
            s.SolicitadoEnUtc,
            s.CompletadoEnUtc,
            contenido);
    }
}

public sealed record ProcesarReporteImpactoCommand(Guid ReporteId) : IRequest<Unit>;

public sealed class ProcesarReporteImpactoHandler : IRequestHandler<ProcesarReporteImpactoCommand, Unit>
{
    private readonly ISolicitudReporteImpactoRepository _solicitudRepo;
    private readonly IRepositorioProyeccionImpacto _proyecciones;
    private readonly IConsentimientoDonanteRepository _consentimientoRepo;
    private readonly IUnidadTrabajoComunicaciones _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public ProcesarReporteImpactoHandler(
        ISolicitudReporteImpactoRepository solicitudRepo,
        IRepositorioProyeccionImpacto proyecciones,
        IConsentimientoDonanteRepository consentimientoRepo,
        IUnidadTrabajoComunicaciones unidadTrabajo,
        IRelojNegocio reloj)
    {
        _solicitudRepo = solicitudRepo;
        _proyecciones = proyecciones;
        _consentimientoRepo = consentimientoRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<Unit> Handle(ProcesarReporteImpactoCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(request.ReporteId, cancellationToken);
        if (solicitud is null || solicitud.Estado == EstadoSolicitudReporteImpacto.LISTO)
        {
            return Unit.Value;
        }

        try
        {
            var aportes = await _proyecciones.ObtenerAportesDonanteEnPeriodoAsync(
                solicitud.DonanteId,
                solicitud.Periodo.Valor,
                cancellationToken);

            var alcance = solicitud.ObtenerProgramasAlcance();
            if (alcance.Count > 0)
            {
                var setAlcance = alcance.ToHashSet();
                aportes = aportes.Where(a => setAlcance.Contains(a.ProgramaId)).ToArray();
            }

            var programaIds = aportes.Select(a => a.ProgramaId).Distinct().ToArray();

            var becarios = await _proyecciones.ObtenerBecariosDeProgramasAsync(programaIds, cancellationToken);
            var desembolsos = await _proyecciones.ObtenerDesembolsosEnPeriodoAsync(programaIds, solicitud.Periodo.Valor, cancellationToken);
            var academicos = await _proyecciones.ObtenerReportesAcademicosEnPeriodoAsync(programaIds, solicitud.Periodo.Valor, cancellationToken);
            var consentimientos = await _consentimientoRepo.ObtenerBecariosConConsentimientoAsync(solicitud.DonanteId, cancellationToken);

            var contenido = ServicioAnonimizacionImpacto.ConstruirReporte(
                solicitud.DonanteId,
                solicitud.NombreDonante,
                solicitud.Periodo.Valor,
                aportes,
                becarios,
                desembolsos,
                academicos,
                consentimientos,
                _reloj.AhoraUtc);

            var json = JsonSerializer.Serialize(contenido, SolicitarReporteImpactoHandler.JsonOptions);
            solicitud.Completar(json, _reloj.AhoraUtc);
        }
        catch (Exception ex)
        {
            solicitud.MarcarFallido(ex.Message, _reloj.AhoraUtc);
        }

        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);
        return Unit.Value;
    }
}

public sealed record ConsultarReporteImpactoQuery(
    ActorSolicitudComunicaciones Actor,
    Guid ReporteId) : IRequest<SolicitudReporteImpactoDto>;

public sealed class ConsultarReporteImpactoHandler
    : IRequestHandler<ConsultarReporteImpactoQuery, SolicitudReporteImpactoDto>
{
    private readonly ISolicitudReporteImpactoRepository _solicitudRepo;

    public ConsultarReporteImpactoHandler(ISolicitudReporteImpactoRepository solicitudRepo) =>
        _solicitudRepo = solicitudRepo;

    public async Task<SolicitudReporteImpactoDto> Handle(
        ConsultarReporteImpactoQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionImpacto.ExigirLecturaReporte(request.Actor);

        var solicitud = await _solicitudRepo.ObtenerPorIdAsync(request.ReporteId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El reporte de impacto indicado no existe.", "REPORTE_NO_ENCONTRADO");

        return SolicitarReporteImpactoHandler.MapearDto(solicitud);
    }
}

public sealed record ListarReportesImpactoQuery(
    ActorSolicitudComunicaciones Actor,
    Guid? DonanteId = null,
    string? CodigoPeriodo = null) : IRequest<IReadOnlyList<SolicitudReporteImpactoDto>>;

public sealed class ListarReportesImpactoHandler
    : IRequestHandler<ListarReportesImpactoQuery, IReadOnlyList<SolicitudReporteImpactoDto>>
{
    private readonly ISolicitudReporteImpactoRepository _solicitudRepo;

    public ListarReportesImpactoHandler(ISolicitudReporteImpactoRepository solicitudRepo) =>
        _solicitudRepo = solicitudRepo;

    public async Task<IReadOnlyList<SolicitudReporteImpactoDto>> Handle(
        ListarReportesImpactoQuery request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionImpacto.ExigirLecturaReporte(request.Actor);

        var lista = await _solicitudRepo.ListarAsync(request.DonanteId, request.CodigoPeriodo, cancellationToken);
        return lista
            .OrderByDescending(s => s.SolicitadoEnUtc)
            .Select(SolicitarReporteImpactoHandler.MapearDto)
            .ToArray();
    }
}
