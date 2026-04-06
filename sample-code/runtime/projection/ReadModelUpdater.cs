using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Persistence;

namespace Whycespace.Runtime.Projection;

/// <summary>
/// Coordinates event-to-read-model updates across registered projection handlers.
/// This is the orchestration layer between the event consumer and the projection store.
///
/// Flow: Event → ReadModelUpdater → ProjectionHandler → IProjectionStore
/// </summary>
public sealed class ReadModelUpdater
{
    private readonly IProjectionStore _store;
    private readonly Dictionary<string, List<ProjectionRegistration>> _handlersByEventType = new();
    private readonly List<ProjectionRegistration> _wildcardHandlers = new();

    public ReadModelUpdater(IProjectionStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    public void Register(ProjectionRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        foreach (var eventType in registration.EventTypes)
        {
            if (eventType == "*")
            {
                _wildcardHandlers.Add(registration);
            }
            else
            {
                if (!_handlersByEventType.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<ProjectionRegistration>();
                    _handlersByEventType[eventType] = handlers;
                }
                handlers.Add(registration);
            }
        }
    }

    /// <summary>
    /// Updates all read models affected by the given event.
    /// </summary>
    public async Task UpdateAsync(RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        // Type-specific handlers
        if (_handlersByEventType.TryGetValue(@event.EventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                await handler.Handler(@event, _store, cancellationToken);
            }
        }

        // Wildcard handlers
        foreach (var handler in _wildcardHandlers)
        {
            await handler.Handler(@event, _store, cancellationToken);
        }
    }

    /// <summary>
    /// Rebuilds all read models by replaying events from the given stream.
    /// </summary>
    public async Task RebuildAsync(IReadOnlyList<RuntimeEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            await UpdateAsync(@event, cancellationToken);
        }
    }

    public int RegisteredProjectionCount =>
        _handlersByEventType.Values.Sum(h => h.Count) + _wildcardHandlers.Count;
}
