using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Subscription.Plan;

public sealed class PlanProjectionHandler
{
    public string ProjectionName => "whyce.business.subscription.plan";

    public string[] EventTypes =>
    [
        "whyce.business.subscription.plan.created",
        "whyce.business.subscription.plan.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPlanViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PlanReadModel
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
