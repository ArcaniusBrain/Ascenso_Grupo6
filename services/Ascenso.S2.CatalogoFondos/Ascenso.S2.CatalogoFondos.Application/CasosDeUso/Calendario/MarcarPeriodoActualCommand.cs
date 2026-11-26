using Ascenso.Contracts.Eventos;
using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Calendario;

public sealed record MarcarPeriodoActualCommand(
    ActorSolicitudCatalogo Actor,
    string CodigoPeriodo,
    Guid OperationId) : IRequest<OperacionConfirmadaDto>;

public sealed class MarcarPeriodoActualHandler : IRequestHandler<MarcarPeriodoActualCommand, OperacionConfirmadaDto>
{
    public const string NombreEventoIntegracion = "PeriodoMarcadoActual.v1";

    private readonly ICalendarioAcademicoRepository _calendarioRepo;
    private readonly IEscritorOutboxCatalogo _outbox;
    private readonly IRegistroIdempotenciaComandos _idempotencia;
    private readonly IUnidadTrabajoCatalogo _unidadTrabajo;
    private readonly IRelojNegocio _reloj;

    public MarcarPeriodoActualHandler(
        ICalendarioAcademicoRepository calendarioRepo,
        IEscritorOutboxCatalogo outbox,
        IRegistroIdempotenciaComandos idempotencia,
        IUnidadTrabajoCatalogo unidadTrabajo,
        IRelojNegocio reloj)
    {
        _calendarioRepo = calendarioRepo;
        _outbox = outbox;
        _idempotencia = idempotencia;
        _unidadTrabajo = unidadTrabajo;
        _reloj = reloj;
    }

    public async Task<OperacionConfirmadaDto> Handle(MarcarPeriodoActualCommand request, CancellationToken cancellationToken)
    {
        PoliticaAutorizacionCalendario.ExigirDireccion(request.Actor);

        if (request.OperationId == Guid.Empty)
        {
            throw new ExcepcionDeDominio(
                "El identificador de operación (operationId) es obligatorio.",
                "OPERATION_ID_OBLIGATORIO");
        }

        // Idempotencia por operationId (ADR-13): un reintento devuelve la misma confirmación sin duplicar eventos.
        var previo = await _idempotencia.BuscarAsync(request.OperationId, cancellationToken);
        if (previo is not null)
        {
            return previo;
        }

        var codigo = new CodigoPeriodo(request.CodigoPeriodo);
        var calendario = await _calendarioRepo.ObtenerInstitucionalAsync(cancellationToken)
            ?? throw new ExcepcionDeDominio($"El periodo {codigo.Valor} no está registrado.", "PERIODO_NO_ENCONTRADO");

        var ahoraUtc = _reloj.AhoraUtc;
        var resultado = calendario.MarcarActual(codigo, ahoraUtc);

        if (!resultado.HuboCambio)
        {
            // HU-38.2 Escenario 8: repetir el periodo ya actual no produce cambios ni eventos duplicados.
            return new OperacionConfirmadaDto(
                calendario.Id,
                "SIN_CAMBIOS",
                request.OperationId,
                calendario.Version);
        }

        var eventoIntegracion = new PeriodoMarcadoActualV1(
            calendario.Id,
            resultado.CodigoPeriodoActual.Valor,
            resultado.CodigoPeriodoAnterior?.Valor);

        await _outbox.RegistrarEventoAsync(
            Guid.NewGuid(),
            NombreEventoIntegracion,
            schemaVersion: 1,
            eventoIntegracion,
            correlationId: request.OperationId,
            causationId: request.OperationId,
            occurredAtUtc: ahoraUtc,
            ct: cancellationToken);

        var confirmacion = new OperacionConfirmadaDto(
            calendario.Id,
            "ACTUALIZADO",
            request.OperationId,
            calendario.Version);

        await _idempotencia.RegistrarAsync(request.OperationId, confirmacion, ahoraUtc, cancellationToken);

        // Persistencia atómica: CalendarioAcademico (Version++) + OutboxMessage + Idempotencia en una sola transacción.
        // Dos llamadas concurrentes compiten por el mismo token de Version -> la segunda falla con DbUpdateConcurrencyException (409).
        await _unidadTrabajo.GuardarCambiosAsync(cancellationToken);

        return confirmacion;
    }
}
