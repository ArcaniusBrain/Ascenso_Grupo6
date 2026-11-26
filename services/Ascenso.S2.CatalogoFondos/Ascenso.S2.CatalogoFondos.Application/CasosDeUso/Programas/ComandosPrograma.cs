using System.Globalization;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Programas;

public sealed record CrearProgramaCommand(
    ActorSolicitudCatalogo Actor,
    string? Nombre,
    string? PeriodoInicio,
    int? CupoBecarios,
    int? DuracionPeriodos,
    decimal? MontoPorPeriodo,
    string? Moneda,
    ModalidadPago? ModalidadPago,
    AlcancePrograma? Alcance,
    bool PermiteAcumulacion = false) : IRequest<ProgramaResumenDto>;

public sealed class CrearProgramaHandler : IRequestHandler<CrearProgramaCommand, ProgramaResumenDto>
{
    public const string MensajeSinPeriodos =
        "No hay periodos académicos registrados. Pide a Dirección que los registre.";

    private readonly IProgramaRepository _programaRepo;
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;
    private readonly ISincronizadorPresupuestoPrograma? _sincronizadorPresupuesto;

    public CrearProgramaHandler(
        IProgramaRepository programaRepo,
        ICalendarioAcademicoRepository calendarioRepo,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj,
        ISincronizadorPresupuestoPrograma? sincronizadorPresupuesto = null)
    {
        _programaRepo = programaRepo;
        _calendarioRepo = calendarioRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
        _sincronizadorPresupuesto = sincronizadorPresupuesto;
    }

    public async Task<ProgramaResumenDto> Handle(CrearProgramaCommand request, CancellationToken cancellationToken)
    {
        PoliticaAutorizacionProgramas.ExigirCreacion(request.Actor);

        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        if (calendario is null || calendario.Periodos.Count == 0)
        {
            throw new ExcepcionDeDominio(MensajeSinPeriodos, "SIN_PERIODOS_REGISTRADOS");
        }

        if (string.IsNullOrWhiteSpace(request.PeriodoInicio))
        {
            throw new ExcepcionDeDominio(
                "Indica el periodo académico al que se liga el programa.",
                "PERIODO_INICIO_OBLIGATORIO");
        }

        var codigoPeriodo = new CodigoPeriodo(request.PeriodoInicio);
        var periodo = calendario.ObtenerPeriodo(codigoPeriodo)
            ?? throw new ExcepcionDeDominio($"El periodo {codigoPeriodo.Valor} no está registrado.", "PERIODO_NO_ENCONTRADO");

        if (periodo.Estado != EstadoPeriodo.ABIERTO)
        {
            throw new ExcepcionDeDominio(
                $"El periodo {codigoPeriodo.Valor} está cerrado y no puede ligarse a un programa nuevo.",
                "PERIODO_CERRADO_NO_ASIGNABLE");
        }

        var programa = Programa.Crear(
            request.Nombre,
            codigoPeriodo,
            request.CupoBecarios,
            request.DuracionPeriodos,
            request.MontoPorPeriodo,
            request.Moneda,
            request.ModalidadPago,
            request.Alcance,
            request.PermiteAcumulacion,
            _reloj.AhoraUtc);

        if (await _programaRepo.ExisteNombreNormalizadoAsync(programa.NombreNormalizado, null, cancellationToken))
        {
            throw new ExcepcionDeDominio(
                $"Ya existe un programa de becas registrado con el nombre '{programa.Nombre}'.",
                "PROGRAMA_DUPLICADO");
        }

        await _programaRepo.AgregarAsync(programa, cancellationToken);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        if (_sincronizadorPresupuesto is not null)
        {
            await _sincronizadorPresupuesto.InicializarParaProgramaAsync(programa, _reloj.AhoraUtc, cancellationToken);
        }

        return MapearResumen(programa);
    }

    internal static ProgramaResumenDto MapearResumen(Programa p) =>
        new(
            p.Id,
            p.Nombre,
            p.PeriodoInicio.Valor,
            p.CupoBecarios,
            p.DuracionPeriodos,
            p.MontoPorPeriodo.Cantidad,
            p.MontoPorPeriodo.Moneda,
            p.ModalidadPago.ToString(),
            p.Alcance.ToString(),
            p.PermiteAcumulacion,
            p.Version);
}

public sealed record ActualizarConfiguracionBaseProgramaCommand(
    ActorSolicitudCatalogo Actor,
    Guid ProgramaId,
    string? Nombre,
    decimal? MontoPorPeriodo,
    string? Moneda,
    ModalidadPago? ModalidadPago,
    AlcancePrograma? Alcance,
    bool PermiteAcumulacion = false) : IRequest<ProgramaResumenDto>;

public sealed class ActualizarConfiguracionBaseProgramaHandler
    : IRequestHandler<ActualizarConfiguracionBaseProgramaCommand, ProgramaResumenDto>
{
    private readonly IProgramaRepository _programaRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;

    public ActualizarConfiguracionBaseProgramaHandler(
        IProgramaRepository programaRepo,
        IUnidadTrabajoCatalogo unidadTrabajo)
    {
        _programaRepo = programaRepo;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<ProgramaResumenDto> Handle(
        ActualizarConfiguracionBaseProgramaCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionProgramas.ExigirGestionPrograma(request.Actor, request.ProgramaId);

        var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

        var claveNueva = UniversidadAliada.NormalizarClaveTexto(request.Nombre);
        if (!string.IsNullOrEmpty(claveNueva)
            && await _programaRepo.ExisteNombreNormalizadoAsync(claveNueva, programa.Id, cancellationToken))
        {
            throw new ExcepcionDeDominio(
                $"Ya existe otro programa de becas registrado con el nombre '{UniversidadAliada.LimpiarNombreVisible(request.Nombre)}'.",
                "PROGRAMA_DUPLICADO");
        }

        programa.ActualizarConfiguracionBase(
            request.Nombre,
            request.MontoPorPeriodo,
            request.Moneda,
            request.ModalidadPago,
            request.Alcance,
            request.PermiteAcumulacion);

        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return CrearProgramaHandler.MapearResumen(programa);
    }
}

public sealed record AsociarDonanteAProgramaCommand(
    ActorSolicitudCatalogo Actor,
    Guid ProgramaId,
    Guid DonanteId) : IRequest<ProgramaDetalleDto>;

public sealed class AsociarDonanteAProgramaHandler
    : IRequestHandler<AsociarDonanteAProgramaCommand, ProgramaDetalleDto>
{
    public const string MensajeSinDonantes = "No hay donantes registrados. Registra uno primero.";

    private readonly IProgramaRepository _programaRepo;
    private readonly IConsultorDonantesYAportes _consultorDonantes;
    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public AsociarDonanteAProgramaHandler(
        IProgramaRepository programaRepo,
        IConsultorDonantesYAportes consultorDonantes,
        IUniversidadAliadaRepository universidadRepo,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _programaRepo = programaRepo;
        _consultorDonantes = consultorDonantes;
        _universidadRepo = universidadRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<ProgramaDetalleDto> Handle(
        AsociarDonanteAProgramaCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionProgramas.ExigirGestionPrograma(request.Actor, request.ProgramaId);

        if (!await _consultorDonantes.ExistenDonantesRegistradosAsync(cancellationToken))
        {
            throw new ExcepcionDeDominio(MensajeSinDonantes, "SIN_DONANTES_REGISTRADOS");
        }

        var donante = await _consultorDonantes.ObtenerDonanteAsync(request.DonanteId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El donante indicado no está registrado.", "DONANTE_NO_ENCONTRADO");

        var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

        programa.AsociarDonante(donante.Id, _reloj.AhoraUtc);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return await ConsultarProgramaDetalleHandler.ConstruirDetalleAsync(
            programa,
            _consultorDonantes,
            _universidadRepo,
            cancellationToken);
    }
}

public sealed record ConfigurarElegibilidadProgramaCommand(
    ActorSolicitudCatalogo Actor,
    Guid ProgramaId,
    IReadOnlyList<RestriccionUniversidadInput>? Restricciones) : IRequest<ProgramaDetalleDto>;

public sealed class ConfigurarElegibilidadProgramaHandler
    : IRequestHandler<ConfigurarElegibilidadProgramaCommand, ProgramaDetalleDto>
{
    public const string MensajeSinUniversidades = "No hay universidades aliadas registradas.";

    private readonly IProgramaRepository _programaRepo;
    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly IConsultorDonantesYAportes _consultorDonantes;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;

    public ConfigurarElegibilidadProgramaHandler(
        IProgramaRepository programaRepo,
        IUniversidadAliadaRepository universidadRepo,
        IConsultorDonantesYAportes consultorDonantes,
        IUnidadTrabajoCatalogo unidadTrabajo)
    {
        _programaRepo = programaRepo;
        _universidadRepo = universidadRepo;
        _consultorDonantes = consultorDonantes;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<ProgramaDetalleDto> Handle(
        ConfigurarElegibilidadProgramaCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionProgramas.ExigirGestionPrograma(request.Actor, request.ProgramaId);

        var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

        var entradas = request.Restricciones ?? [];
        var listaValidada = new List<(Guid UniversidadId, string NombreUniversidad, string? CarreraNombre)>();

        if (entradas.Count > 0)
        {
            var todasUniversidades = await _universidadRepo.ListarTodasAsync(cancellationToken);
            if (todasUniversidades.Count == 0)
            {
                throw new ExcepcionDeDominio(MensajeSinUniversidades, "SIN_UNIVERSIDADES_ALIADAS");
            }

            foreach (var item in entradas)
            {
                var universidad = await _universidadRepo.ObtenerPorIdAsync(item.UniversidadId, cancellationToken)
                    ?? throw new ExcepcionDeDominio(
                        "La universidad aliada indicada no está registrada.",
                        "UNIVERSIDAD_NO_ENCONTRADA");

                var carrerasSolicitadas = (item.Carreras ?? [])
                    .Select(c => (c ?? string.Empty).Trim())
                    .Where(c => c.Length > 0)
                    .ToArray();

                if (carrerasSolicitadas.Length == 0)
                {
                    listaValidada.Add((universidad.Id, universidad.Nombre, null));
                    continue;
                }

                foreach (var carreraSolicitada in carrerasSolicitadas)
                {
                    var clave = UniversidadAliada.NormalizarClaveTexto(carreraSolicitada);
                    var carreraRegistrada = universidad.Carreras.FirstOrDefault(c => c.NombreNormalizado == clave);
                    if (carreraRegistrada is null)
                    {
                        throw new ExcepcionDeDominio(
                            $"La carrera '{carreraSolicitada}' no está registrada como elegible en {universidad.Nombre}.",
                            "CARRERA_NO_ELEGIBLE");
                    }

                    listaValidada.Add((universidad.Id, universidad.Nombre, carreraRegistrada.Nombre));
                }
            }
        }

        programa.ReemplazarRestricciones(listaValidada);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return await ConsultarProgramaDetalleHandler.ConstruirDetalleAsync(
            programa,
            _consultorDonantes,
            _universidadRepo,
            cancellationToken);
    }
}

public sealed record ConfigurarCondicionesProgramaCommand(
    ActorSolicitudCatalogo Actor,
    Guid ProgramaId,
    string? PromedioMinimoTexto,
    bool ExigeHorasServicio,
    int? HorasServicio,
    IReadOnlyList<string>? ReportesExigidos,
    int? DuracionMaxPausaMeses,
    string? DescripcionRenovacion) : IRequest<ProgramaDetalleDto>;

public sealed class ConfigurarCondicionesProgramaHandler
    : IRequestHandler<ConfigurarCondicionesProgramaCommand, ProgramaDetalleDto>
{
    private readonly IProgramaRepository _programaRepo;
    private readonly IConsultorDonantesYAportes _consultorDonantes;
    private readonly IUniversidadAliadaRepository _universidadRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;

    public ConfigurarCondicionesProgramaHandler(
        IProgramaRepository programaRepo,
        IConsultorDonantesYAportes consultorDonantes,
        IUniversidadAliadaRepository universidadRepo,
        IUnidadTrabajoCatalogo unidadTrabajo)
    {
        _programaRepo = programaRepo;
        _consultorDonantes = consultorDonantes;
        _universidadRepo = universidadRepo;
        _unidadTrabajo = unidadTrabajo;
    }

    public async Task<ProgramaDetalleDto> Handle(
        ConfigurarCondicionesProgramaCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionProgramas.ExigirGestionPrograma(request.Actor, request.ProgramaId);

        var programa = await _programaRepo.ObtenerPorIdAsync(request.ProgramaId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

        var promedio = ParsearPromedio(request.PromedioMinimoTexto);
        var condiciones = CondicionesPermanencia.Crear(
            promedio,
            request.ExigeHorasServicio,
            request.HorasServicio,
            request.ReportesExigidos,
            request.DuracionMaxPausaMeses,
            request.DescripcionRenovacion);

        programa.ConfigurarCondiciones(condiciones);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return await ConsultarProgramaDetalleHandler.ConstruirDetalleAsync(
            programa,
            _consultorDonantes,
            _universidadRepo,
            cancellationToken);
    }

    private static decimal? ParsearPromedio(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        var normalizado = texto.Trim().Replace(',', '.');
        return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : null;
    }
}
