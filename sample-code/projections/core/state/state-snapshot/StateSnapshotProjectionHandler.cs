using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.State.StateSnapshot;

public sealed class StateSnapshotProjectionHandler
{
    public string ProjectionName => "whyce.core.state.state-snapshot";

    public string[] EventTypes =>
    [
        "whyce.core.state.state-snapshot.created",
        "whyce.core.state.state-snapshot.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStateSnapshotViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StateSnapshotReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
