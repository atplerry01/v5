using Whycespace.Runtime.EventFabric;

namespace Whycespace.Runtime.Projection;

public sealed class ProjectionEventConsumer : IEventConsumer
{
    private readonly ProjectionEngine _projectionEngine;
    private readonly IProjectionErrorHandler? _errorHandler;

    public ProjectionEventConsumer(
        ProjectionEngine projectionEngine,
        IProjectionErrorHandler? errorHandler = null)
    {
        ArgumentNullException.ThrowIfNull(projectionEngine);
        _projectionEngine = projectionEngine;
        _errorHandler = errorHandler;
    }

    public string EventType => "*";

    public async Task HandleAsync(RuntimeEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            await _projectionEngine.ProjectAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful shutdown — do not log as error.
        }
        catch (Exception ex)
        {
            // Projection failure must not crash the event pipeline.
            // Checkpoint-based idempotency in ProjectionEngine guarantees
            // catch-up on the next event.
            _errorHandler?.OnProjectionError(@event, ex);
        }
    }
}

/// <summary>
/// Optional error handler for projection failures.
/// Implementations can log, emit metrics, or alert.
/// </summary>
public interface IProjectionErrorHandler
{
    void OnProjectionError(RuntimeEvent @event, Exception exception);
}
