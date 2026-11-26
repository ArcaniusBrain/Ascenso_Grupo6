using Ascenso.Contracts.Eventos;
using Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Donantes;

public sealed record RegistrarDonanteCommand(
    ActorSolicitudCatalogo Actor,
    string? Nombre,
    TipoDonante? Tipo,
    ContactoDonanteDto? Contacto) : IRequest<DonanteResumenDto>;

public sealed class RegistrarDonanteHandler
    : IRequestHandler<RegistrarDonanteCommand, DonanteResumenDto>
{
    private readonly IDonanteRepository _donanteRepo;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarDonanteHandler(
        IDonanteRepository donanteRepo,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _donanteRepo = donanteRepo;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<DonanteResumenDto> Handle(
        RegistrarDonanteCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionDonantes.ExigirGestionDonante(request.Actor);

        var contacto = request.Contacto is null
            ? null
            : new ContactoDonante(request.Contacto.Correo, request.Contacto.Telefono);

        var donante = Donante.Crear(
            request.Nombre,
            request.Tipo,
            contacto,
            _reloj.AhoraUtc);

        if (await _donanteRepo.ExisteNombreNormalizadoAsync(donante.NombreNormalizado, null, cancellationToken))
        {
            throw new ExcepcionDeDominio(
                $"Ya existe un donante registrado con el nombre '{donante.Nombre}'.",
                "DONANTE_DUPLICADO");
        }

        await _donanteRepo.AgregarAsync(donante, cancellationToken);
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return MapearResumen(donante);
    }

    internal static DonanteResumenDto MapearResumen(Donante d) =>
        new(
            d.Id,
            d.Nombre,
            d.Tipo.ToString(),
            new ContactoDonanteDto(d.Contacto.Correo, d.Contacto.Telefono),
            d.Aportes.Count,
            d.TotalAportado(),
            d.Version);
}

public sealed record RegistrarAporteCommand(
    ActorSolicitudCatalogo Actor,
    Guid DonanteId,
    Guid ProgramaId,
    decimal? Cantidad,
    string? Moneda,
    string? CodigoPeriodo) : IRequest<AporteRegistradoResultadoDto>;

public sealed class RegistrarAporteHandler
    : IRequestHandler<RegistrarAporteCommand, AporteRegistradoResultadoDto>
{
    public const string NombreEventoIntegracion = "AporteRegistrado.v1";
    public const string MensajeSinProgramas = "No hay programas registrados.";

    private readonly IDonanteRepository _donanteRepo;
    private readonly IProgramaRepository _programaRepo;
    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IEscritorOutboxCatalogo _outbox;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public RegistrarAporteHandler(
        IDonanteRepository donanteRepo,
        IProgramaRepository programaRepo,
        ICalendarioAcademicoRepository calendarioRepo,
        IEscritorOutboxCatalogo outbox,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _donanteRepo = donanteRepo;
        _programaRepo = programaRepo;
        _calendarioRepo = calendarioRepo;
        _outbox = outbox;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<AporteRegistradoResultadoDto> Handle(
        RegistrarAporteCommand request,
        CancellationToken cancellationToken)
    {
        PoliticaAutorizacionDonantes.ExigirRegistroAporte(request.Actor, request.ProgramaId);

        var programas = await _programaRepo.ListarTodosAsync(cancellationToken);
        if (programas.Count == 0)
        {
            throw new ExcepcionDeDominio(MensajeSinProgramas, "SIN_PROGRAMAS_REGISTRADOS");
        }

        if (string.IsNullOrWhiteSpace(request.CodigoPeriodo))
        {
            throw new ExcepcionDeDominio(Donante.MensajePeriodoObligatorio, "PERIODO_APORTE_OBLIGATORIO");
        }

        if (request.Cantidad is null || request.Cantidad.Value <= 0m)
        {
            throw new ExcepcionDeDominio(Donante.MensajeMontoDebeSerPositivo, "MONTO_APORTE_INVALIDO");
        }

        var programa = programas.FirstOrDefault(p => p.Id == request.ProgramaId)
            ?? throw new ExcepcionDeDominio("El programa de becas indicado no está registrado.", "PROGRAMA_NO_ENCONTRADO");

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken);
        if (calendario is null || !calendario.ExistePeriodo(codigo))
        {
            throw new ExcepcionDeDominio($"El periodo {codigo.Valor} no está registrado.", "PERIODO_NO_ENCONTRADO");
        }

        var donante = await _donanteRepo.ObtenerPorIdAsync(request.DonanteId, cancellationToken)
            ?? throw new ExcepcionDeDominio("El donante indicado no está registrado.", "DONANTE_NO_ENCONTRADO");

        var ahoraUtc = _reloj.AhoraUtc;
        var aporte = donante.RegistrarAporte(
            programa.Id,
            codigo,
            request.Cantidad.Value,
            request.Moneda,
            ahoraUtc);

        var evento = new AporteRegistradoV1(
            aporte.Id,
            donante.Id,
            donante.Nombre,
            programa.Id,
            aporte.Periodo.Valor,
            aporte.Monto.Cantidad,
            aporte.Monto.Moneda,
            aporte.RegistradoEnUtc);

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

        var aporteDto = new AporteDto(
            aporte.Id,
            donante.Id,
            donante.Nombre,
            programa.Id,
            programa.Nombre,
            aporte.Periodo.Valor,
            aporte.Monto.Cantidad,
            aporte.Monto.Moneda,
            aporte.RegistradoEnUtc);

        var confirmacion = new OperacionConfirmadaDto(
            aporte.Id,
            "REGISTRADO",
            correlationId,
            donante.Version);

        return new AporteRegistradoResultadoDto(aporteDto, confirmacion);
    }
}
