namespace Whycespace.Runtime.EventFabric;

public interface IEventConsumer
{
    string EventType { get; }
    Task HandleAsync(RuntimeEvent @event, CancellationToken cancellationToken = default);
}
