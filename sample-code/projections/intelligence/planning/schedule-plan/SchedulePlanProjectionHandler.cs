using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Planning.SchedulePlan;

public sealed class SchedulePlanProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.planning.schedule-plan";

    public string[] EventTypes =>
    [
        "whyce.intelligence.planning.schedule-plan.created",
        "whyce.intelligence.planning.schedule-plan.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISchedulePlanViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SchedulePlanReadModel
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
