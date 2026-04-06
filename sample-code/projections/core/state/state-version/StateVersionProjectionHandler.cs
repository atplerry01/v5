using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.State.StateVersion;

public sealed class StateVersionProjectionHandler
{
    public string ProjectionName => "whyce.core.state.state-version";

    public string[] EventTypes =>
    [
        "whyce.core.state.state-version.created",
        "whyce.core.state.state-version.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStateVersionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StateVersionReadModel
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
