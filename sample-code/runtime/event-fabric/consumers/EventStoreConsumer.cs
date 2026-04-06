using System.Collections.Concurrent;
using Whycespace.Runtime.Persistence;

namespace Whycespace.Runtime.EventFabric.Consumers;

public sealed class EventStoreConsumer : IEventConsumer
{
    private readonly IEventStore _eventStore;
    private readonly ConcurrentDictionary<Guid, byte> _processed = new();

    public EventStoreConsumer(IEventStore eventStore)
    {
        ArgumentNullException.ThrowIfNull(eventStore);
        _eventStore = eventStore;
    }

    public string EventType => "*";

    public async Task HandleAsync(RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        if (!_processed.TryAdd(@event.EventId, 0))
            return;

        var streamId = @event.CorrelationId;

        await _eventStore.AppendAsync(streamId, @event, cancellationToken);
    }
}
