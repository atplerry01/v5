namespace Whycespace.Runtime.EventFabric;

public sealed class EventConsumer : IEventConsumer
{
    private readonly Func<RuntimeEvent, CancellationToken, Task> _handler;

    public string EventType { get; }

    public EventConsumer(string eventType, Func<RuntimeEvent, CancellationToken, Task> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentNullException.ThrowIfNull(handler);

        EventType = eventType;
        _handler = handler;
    }

    public Task HandleAsync(RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        return _handler(@event, cancellationToken);
    }
}
