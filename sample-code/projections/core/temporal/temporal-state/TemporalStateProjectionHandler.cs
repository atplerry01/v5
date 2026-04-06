using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Temporal.TemporalState;

public sealed class TemporalStateProjectionHandler
{
    public string ProjectionName => "whyce.core.temporal.temporal-state";

    public string[] EventTypes =>
    [
        "whyce.core.temporal.temporal-state.created",
        "whyce.core.temporal.temporal-state.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITemporalStateViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TemporalStateReadModel
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
