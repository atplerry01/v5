using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Subscription.Usage;

public sealed class UsageProjectionHandler
{
    public string ProjectionName => "whyce.business.subscription.usage";

    public string[] EventTypes =>
    [
        "whyce.business.subscription.usage.created",
        "whyce.business.subscription.usage.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IUsageViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new UsageReadModel
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
