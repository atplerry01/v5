using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Estimation.CostEstimate;

public sealed class CostEstimateProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.estimation.cost-estimate";

    public string[] EventTypes =>
    [
        "whyce.intelligence.estimation.cost-estimate.created",
        "whyce.intelligence.estimation.cost-estimate.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICostEstimateViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CostEstimateReadModel
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
