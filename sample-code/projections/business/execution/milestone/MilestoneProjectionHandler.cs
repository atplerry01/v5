using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Milestone;

public sealed class MilestoneProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.milestone";

    public string[] EventTypes =>
    [
        "whyce.business.execution.milestone.created",
        "whyce.business.execution.milestone.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IMilestoneViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new MilestoneReadModel
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
