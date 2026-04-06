using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Revenue;

public sealed class RevenueProjectionHandler
{
    public string ProjectionName => "whyce.economic.revenue.revenue";

    public string[] EventTypes =>
    [
        "whyce.economic.revenue.revenue.created",
        "whyce.economic.revenue.revenue.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRevenueViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RevenueReadModel
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
