using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Estimation.PriceEstimate;

public sealed class PriceEstimateProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.estimation.price-estimate";

    public string[] EventTypes =>
    [
        "whyce.intelligence.estimation.price-estimate.created",
        "whyce.intelligence.estimation.price-estimate.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPriceEstimateViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PriceEstimateReadModel
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
