using Whycespace.Runtime.Projection;

namespace Whycespace.Runtime.EventFabric.Workers;

/// <summary>
/// Projection Async Worker — processes projection handlers asynchronously
/// for event types configured with ASYNC execution policy.
/// Decouples projection updates from the main execution path.
/// </summary>
public sealed class ProjectionAsyncWorker
{
    private readonly IProjectionDispatcher _projectionDispatcher;

    public ProjectionAsyncWorker(IProjectionDispatcher projectionDispatcher)
    {
        _projectionDispatcher = projectionDispatcher;
    }

    public async Task ProcessAsync(IReadOnlyList<EventEnvelope> envelopes)
    {
        if (envelopes.Count == 0) return;
        await _projectionDispatcher.DispatchAsync(envelopes);
    }
}
