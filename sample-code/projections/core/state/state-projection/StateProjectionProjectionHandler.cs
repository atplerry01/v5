using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.State.StateProjection;

public sealed class StateProjectionProjectionHandler
{
    public string ProjectionName => "whyce.core.state.state-projection";

    public string[] EventTypes =>
    [
        "whyce.core.state.state-projection.created",
        "whyce.core.state.state-projection.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStateProjectionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StateProjectionReadModel
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
