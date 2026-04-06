using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Assignment;

public sealed class AssignmentProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.assignment";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.assignment.created",
        "whyce.orchestration.workflow.assignment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAssignmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AssignmentReadModel
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
