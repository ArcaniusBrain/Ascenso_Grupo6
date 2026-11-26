using Ascenso.S2.CatalogoFondos.Application.Puertos;
using Ascenso.S2.CatalogoFondos.Domain.Agregados;
using Ascenso.S2.CatalogoFondos.Domain.Repositorios;
using Ascenso.SharedKernel.Tiempo;
using MediatR;

namespace Ascenso.S2.CatalogoFondos.Application.CasosDeUso.Presupuesto;

/// <summary>
/// Reconciliador seguro de reservas sin vincular (ADR-26 / C5):
/// - Evalúa únicamente reservas en estado RESERVADA con más de 30 minutos de antigüedad.
/// - Consulta obligatoriamente a S4 (GET /internal/v1/reservas/{reservaId}/vinculo).
/// - Si S4 responde existeVinculo=true -> vincula idempotentemente la reserva.
/// - Si S4 responde existeVinculo=false -> libera la reserva con motivo COMPENSACION.
/// - Si S4 no responde o falla -> NO libera (fail closed) y reintenta en el próximo ciclo.
/// </summary>
public sealed record ConciliarReservasHuerfanasCommand : IRequest<ResultadoConciliacionReservasDto>;

public sealed class ConciliarReservasHuerfanasHandler
    : IRequestHandler<ConciliarReservasHuerfanasCommand, ResultadoConciliacionReservasDto>
{
    public const int PlazoGraciaMinutos = 30;

    private readonly IPresupuestoProgramaRepository _presupuestoRepo;
    private readonly IConsultorVinculoReservaS4 _consultorS4;
    private readonly ISender _sender;
    private readonly IRelojNegocio _reloj;

    public ConciliarReservasHuerfanasHandler(
        IPresupuestoProgramaRepository presupuestoRepo,
        IConsultorVinculoReservaS4 consultorS4,
        ISender sender,
        IRelojNegocio reloj)
    {
        _presupuestoRepo = presupuestoRepo;
        _consultorS4 = consultorS4;
        _sender = sender;
        _reloj = reloj;
    }

    public async Task<ResultadoConciliacionReservasDto> Handle(
        ConciliarReservasHuerfanasCommand request,
        CancellationToken cancellationToken)
    {
        var limiteUtc = _reloj.AhoraUtc.AddMinutes(-PlazoGraciaMinutos);
        var candidatas = await _presupuestoRepo.ListarReservasSinVinculoAnterioresAAsync(limiteUtc, cancellationToken);

        var vinculadas = 0;
        var liberadas = 0;
        var omitidas = 0;

        foreach (var reserva in candidatas)
        {
            var consulta = await _consultorS4.ConsultarVinculoAsync(reserva.Id, cancellationToken);
            if (!consulta.Disponible || consulta.Estado is null)
            {
                // ADR-26 fail closed: nunca liberar si S4 no responde de forma autoritativa.
                omitidas++;
                continue;
            }

            if (consulta.Estado.ExisteVinculo)
            {
                await _sender.Send(
                    new VincularReservaCommand(reserva.Id, consulta.Estado.RecursoId),
                    cancellationToken);
                vinculadas++;
            }
            else
            {
                await _sender.Send(
                    new LiberarReservaInternaCommand(
                        reserva.Id,
                        Guid.NewGuid(),
                        MotivoLiberacionReserva.COMPENSACION),
                    cancellationToken);
                liberadas++;
            }
        }

        return new ResultadoConciliacionReservasDto(candidatas.Count, vinculadas, liberadas, omitidas);
    }
}
