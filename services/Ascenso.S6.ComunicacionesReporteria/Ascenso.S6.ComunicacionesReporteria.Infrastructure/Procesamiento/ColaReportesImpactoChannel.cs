using System.Threading.Channels;
using Ascenso.S6.ComunicacionesReporteria.Application.Puertos;

namespace Ascenso.S6.ComunicacionesReporteria.Infrastructure.Procesamiento;

/// <summary>
/// Cola asíncrona en memoria (Channel) para desacoplar la solicitud HTTP de la generación pesada de reportes de impacto (HU-33.1 E9, E12).
/// </summary>
public sealed class ColaReportesImpactoChannel : IColaProcesamientoReportesImpacto
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ChannelReader<Guid> Reader => _channel.Reader;

    public ValueTask EncolarAsync(Guid reporteId, CancellationToken ct = default) =>
        _channel.Writer.WriteAsync(reporteId, ct);
}
