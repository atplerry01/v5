using Whycespace.Runtime.EventFabric;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Runtime.Projection;

/// <summary>
/// Projection Dispatcher — routes event envelopes to registered projection handlers.
/// Respects ProjectionExecutionPolicy: INLINE handlers run synchronously,
/// ASYNC handlers are queued for background processing.
/// </summary>
public sealed class ProjectionDispatcher : IProjectionDispatcher
{
    private readonly ProjectionRegistry _registry;

    public ProjectionDispatcher(ProjectionRegistry registry)
    {
        _registry = registry;
    }

    public async Task DispatchAsync(IReadOnlyList<EventEnvelope> envelopes)
    {
        foreach (var envelope in envelopes)
        {
            var handlers = _registry.ResolveHandlers(envelope.EventType);
            foreach (var handler in handlers)
            {
                if (handler.ExecutionPolicy == ProjectionExecutionPolicy.Inline)
                {
                    await handler.HandleAsync(envelope);
                }
                // ASYNC handlers are processed by ProjectionAsyncWorker
            }
        }
    }
}
