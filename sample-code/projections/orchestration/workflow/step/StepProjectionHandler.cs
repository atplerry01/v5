using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Step;

public sealed class StepProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.step";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.step.created",
        "whyce.orchestration.workflow.step.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStepViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StepReadModel
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
