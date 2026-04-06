using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Cost.CostDriver;

public sealed class CostDriverProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.cost.cost-driver";

    public string[] EventTypes =>
    [
        "whyce.intelligence.cost.cost-driver.created",
        "whyce.intelligence.cost.cost-driver.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICostDriverViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CostDriverReadModel
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
