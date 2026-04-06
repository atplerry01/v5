namespace Whycespace.Runtime.EventFabric;

public interface IEventPublisher
{
    Task PublishAsync(RuntimeEvent @event, CancellationToken cancellationToken = default);
    Task PublishAsync(IEnumerable<RuntimeEvent> events, CancellationToken cancellationToken = default);
}
