using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Planning.CapacityPlan;

public sealed class CapacityPlanProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.planning.capacity-plan";

    public string[] EventTypes =>
    [
        "whyce.intelligence.planning.capacity-plan.created",
        "whyce.intelligence.planning.capacity-plan.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICapacityPlanViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CapacityPlanReadModel
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
