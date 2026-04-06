using System.Collections.Concurrent;

namespace Whycespace.Runtime.EventFabric;

public sealed class EventRouter
{
    private readonly ConcurrentDictionary<string, ConcurrentBag<IEventConsumer>> _subscriptions = new();
    private readonly ConcurrentBag<IEventConsumer> _wildcardConsumers = new();
    private volatile IEventConsumer? _persistenceConsumer;

    /// <summary>
    /// Registers a persistence consumer that executes before all other consumers.
    /// Persistence failure halts routing — no downstream consumers are notified.
    /// </summary>
    public void UsePersistenceConsumer(IEventConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);

        if (Interlocked.CompareExchange(ref _persistenceConsumer, consumer, null) is not null)
            throw new InvalidOperationException("A persistence consumer is already registered.");
    }

    public void Subscribe(IEventConsumer consumer)
    {
        ArgumentNullException.ThrowIfNull(consumer);

        if (consumer.EventType == "*")
        {
            _wildcardConsumers.Add(consumer);
            return;
        }

        var bag = _subscriptions.GetOrAdd(consumer.EventType, _ => new ConcurrentBag<IEventConsumer>());
        bag.Add(consumer);
    }

    internal async Task RouteAsync(RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        // Persistence-first: must succeed before any downstream consumer
        if (_persistenceConsumer is not null)
        {
            await _persistenceConsumer.HandleAsync(@event, cancellationToken);
        }

        // Typed consumers
        if (_subscriptions.TryGetValue(@event.EventType, out var consumers))
        {
            foreach (var consumer in consumers)
            {
                await consumer.HandleAsync(@event, cancellationToken);
            }
        }

        // Wildcard consumers
        foreach (var consumer in _wildcardConsumers)
        {
            await consumer.HandleAsync(@event, cancellationToken);
        }
    }
}
