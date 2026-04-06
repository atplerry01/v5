using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Revenue.Distribution;

public sealed class DistributionProjectionHandler
{
    public string ProjectionName => "whyce.economic.revenue.distribution";

    public string[] EventTypes =>
    [
        "whyce.economic.revenue.distribution.created",
        "whyce.economic.revenue.distribution.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDistributionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DistributionReadModel
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
