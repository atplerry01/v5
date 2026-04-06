using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Compensation;

public sealed class CompensationProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.compensation";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.compensation.created",
        "whyce.orchestration.workflow.compensation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICompensationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CompensationReadModel
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
