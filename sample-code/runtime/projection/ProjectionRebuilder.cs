using Whycespace.Runtime.Persistence;

namespace Whycespace.Runtime.Projection;

/// <summary>
/// Rebuilds projections by replaying events from the event store.
/// Resets checkpoints before replay to ensure a clean rebuild.
/// </summary>
public sealed class ProjectionRebuilder
{
    private readonly IEventStore _eventStore;
    private readonly ProjectionEngine _projectionEngine;
    private readonly IProjectionStore _projectionStore;

    public ProjectionRebuilder(
        IEventStore eventStore,
        ProjectionEngine projectionEngine,
        IProjectionStore projectionStore)
    {
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(projectionEngine);
        ArgumentNullException.ThrowIfNull(projectionStore);

        _eventStore = eventStore;
        _projectionEngine = projectionEngine;
        _projectionStore = projectionStore;
    }

    /// <summary>
    /// Rebuilds projections for a single stream by replaying all its events.
    /// </summary>
    public async Task RebuildStreamAsync(string streamId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamId);

        await _projectionEngine.ProjectStreamAsync(streamId, cancellationToken);
    }

    /// <summary>
    /// Full rebuild: resets all projection checkpoints to zero, then replays all events.
    /// </summary>
    public async Task RebuildAllAsync(CancellationToken cancellationToken = default)
    {
        // Reset checkpoints so ProjectAsync replays from the beginning
        await ResetCheckpointsAsync(cancellationToken);

        // Replay all events through the projection engine
        await _projectionEngine.ProjectAsync(cancellationToken);
    }

    /// <summary>
    /// Rebuilds projections from a specific position (partial rebuild).
    /// </summary>
    public async Task RebuildFromAsync(DateTimeOffset after, CancellationToken cancellationToken = default)
    {
        var events = await _eventStore.ReadAllAsync(after, cancellationToken);

        foreach (var @event in events)
        {
            await _projectionEngine.ProjectStreamAsync(@event.CorrelationId, cancellationToken);
        }
    }

    private async Task ResetCheckpointsAsync(CancellationToken cancellationToken)
    {
        // Reset checkpoints for all registered projections to trigger full replay.
        // The ProjectionEngine reads checkpoints to determine where to start,
        // so setting them to 0 forces a complete replay.
        foreach (var registration in _projectionEngine.Registrations)
        {
            await _projectionStore.SetCheckpointAsync(registration.ProjectionName, 0, cancellationToken);
        }
    }
}
