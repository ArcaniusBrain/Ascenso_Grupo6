using MediatR;

namespace Ascenso.SharedKernel.TiposBase;

public interface IEventoDeDominio : INotification
{
    DateTimeOffset Fecha { get; }
}
