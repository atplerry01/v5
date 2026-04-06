using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.State.StateTransition;

public sealed class StateTransitionProjectionHandler
{
    public string ProjectionName => "whyce.core.state.state-transition";

    public string[] EventTypes =>
    [
        "whyce.core.state.state-transition.created",
        "whyce.core.state.state-transition.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStateTransitionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StateTransitionReadModel
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
