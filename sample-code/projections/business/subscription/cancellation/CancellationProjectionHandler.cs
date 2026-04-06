using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Subscription.Cancellation;

public sealed class CancellationProjectionHandler
{
    public string ProjectionName => "whyce.business.subscription.cancellation";

    public string[] EventTypes =>
    [
        "whyce.business.subscription.cancellation.created",
        "whyce.business.subscription.cancellation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICancellationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CancellationReadModel
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
