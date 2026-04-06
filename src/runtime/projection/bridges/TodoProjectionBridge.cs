using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Events.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;

namespace Whyce.Runtime.Projection.Bridges;

/// <summary>
/// Bridges the envelope-based ProjectionRegistry dispatch to the typed
/// TodoProjectionHandler. Unwraps EventEnvelope.Payload and delegates
/// to the appropriate HandleAsync overload.
///
/// Lives in runtime layer because it needs EventEnvelope (runtime type).
/// Delegates to IProjectionHandler&lt;T&gt; implementations in the projections layer.
///
/// Unmatched event types THROW to prevent silent failures.
/// </summary>
public sealed class TodoProjectionBridge : IProjectionHandler
{
    private readonly IProjectionHandler<TodoCreatedEventSchema> _createdHandler;
    private readonly IProjectionHandler<TodoUpdatedEventSchema> _updatedHandler;
    private readonly IProjectionHandler<TodoCompletedEventSchema> _completedHandler;

    public TodoProjectionBridge(
        IProjectionHandler<TodoCreatedEventSchema> createdHandler,
        IProjectionHandler<TodoUpdatedEventSchema> updatedHandler,
        IProjectionHandler<TodoCompletedEventSchema> completedHandler)
    {
        _createdHandler = createdHandler;
        _updatedHandler = updatedHandler;
        _completedHandler = completedHandler;
    }

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public async Task HandleAsync(EventEnvelope envelope)
    {
        switch (envelope.Payload)
        {
            case TodoCreatedEventSchema created:
                await _createdHandler.HandleAsync(created);
                break;

            case TodoUpdatedEventSchema updated:
                await _updatedHandler.HandleAsync(updated);
                break;

            case TodoCompletedEventSchema completed:
                await _completedHandler.HandleAsync(completed);
                break;

            default:
                throw new InvalidOperationException(
                    $"TodoProjectionBridge received unmatched event type: {envelope.Payload.GetType().Name}. " +
                    $"EventId={envelope.EventId}, EventType={envelope.EventType}. " +
                    $"All events dispatched to this projection MUST be handled.");
        }
    }
}
