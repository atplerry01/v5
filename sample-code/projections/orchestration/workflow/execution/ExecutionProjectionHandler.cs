using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Execution;

public sealed class ExecutionProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.execution";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.execution.created",
        "whyce.orchestration.workflow.execution.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IExecutionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ExecutionReadModel
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
