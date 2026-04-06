using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Cost.CostModel;

public sealed class CostModelProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.cost.cost-model";

    public string[] EventTypes =>
    [
        "whyce.intelligence.cost.cost-model.created",
        "whyce.intelligence.cost.cost-model.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICostModelViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CostModelReadModel
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
