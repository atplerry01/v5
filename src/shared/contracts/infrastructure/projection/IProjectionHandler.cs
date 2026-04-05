namespace Whyce.Shared.Contracts.Infrastructure.Projection;

public interface IProjectionHandler<in TEvent> where TEvent : class
{
    Task HandleAsync(TEvent @event);
}
