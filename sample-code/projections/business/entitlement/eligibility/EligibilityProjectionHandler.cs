using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.Eligibility;

public sealed class EligibilityProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.eligibility";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.eligibility.created",
        "whyce.business.entitlement.eligibility.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEligibilityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EligibilityReadModel
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
