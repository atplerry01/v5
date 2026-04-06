using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Subscription.Renewal;

public sealed class RenewalProjectionHandler
{
    public string ProjectionName => "whyce.business.subscription.renewal";

    public string[] EventTypes =>
    [
        "whyce.business.subscription.renewal.created",
        "whyce.business.subscription.renewal.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRenewalViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RenewalReadModel
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
