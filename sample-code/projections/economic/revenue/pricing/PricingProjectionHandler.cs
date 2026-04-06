using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Pricing;

public sealed class PricingProjectionHandler
{
    public string ProjectionName => "whyce.economic.revenue.pricing";

    public string[] EventTypes =>
    [
        "whyce.economic.revenue.pricing.created",
        "whyce.economic.revenue.pricing.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPricingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PricingReadModel
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
