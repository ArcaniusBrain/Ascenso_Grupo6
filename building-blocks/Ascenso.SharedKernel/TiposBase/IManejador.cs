namespace Ascenso.SharedKernel.TiposBase;

public interface IManejador<in TEvento>
    where TEvento : IEventoDeDominio
{
    Task ManejarAsync(TEvento evento, CancellationToken cancellationToken = default);
}
