using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.UsageRight;

public sealed class UsageRightProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.usage-right";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.usage-right.created",
        "whyce.business.entitlement.usage-right.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IUsageRightViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new UsageRightReadModel
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
