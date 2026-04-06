using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Subscription;

public sealed class SubscriptionProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.subscription";

    public string[] EventTypes =>
    [
        "whyce.business.integration.subscription.created",
        "whyce.business.integration.subscription.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISubscriptionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SubscriptionReadModel
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
