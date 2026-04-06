using Whycespace.Shared.Contracts.Event;

namespace Whycespace.Shared.Contracts.Projection;

public interface IProjectionHandler
{
    Task HandleAsync(IEvent @event, CancellationToken cancellationToken);
}
