using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.Quota;

public sealed class QuotaProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.quota";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.quota.created",
        "whyce.business.entitlement.quota.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IQuotaViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new QuotaReadModel
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
