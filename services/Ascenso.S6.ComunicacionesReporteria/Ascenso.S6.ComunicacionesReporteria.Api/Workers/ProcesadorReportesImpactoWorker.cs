using Ascenso.S6.ComunicacionesReporteria.Application.CasosDeUso.ReportesImpacto;
using Ascenso.S6.ComunicacionesReporteria.Infrastructure.Procesamiento;
using MediatR;

namespace Ascenso.S6.ComunicacionesReporteria.Api.Workers;

/// <summary>
/// Worker en segundo plano que procesa la generación asíncrona de reportes de impacto sin bloquear la operación diaria (HU-33.1 E9, E12).
/// </summary>
public sealed class ProcesadorReportesImpactoWorker : BackgroundService
{
    private readonly ColaReportesImpactoChannel _cola;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProcesadorReportesImpactoWorker(
        ColaReportesImpactoChannel cola,
        IServiceScopeFactory scopeFactory)
    {
        _cola = cola;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var reporteId in _cola.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new ProcesarReporteImpactoCommand(reporteId), stoppingToken);
        }
    }
}
