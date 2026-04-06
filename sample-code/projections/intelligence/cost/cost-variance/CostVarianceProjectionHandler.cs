using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Cost.CostVariance;

public sealed class CostVarianceProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.cost.cost-variance";

    public string[] EventTypes =>
    [
        "whyce.intelligence.cost.cost-variance.created",
        "whyce.intelligence.cost.cost-variance.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICostVarianceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CostVarianceReadModel
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
