using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Stage;

public sealed class StageProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.stage";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.stage.created",
        "whyce.orchestration.workflow.stage.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStageViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StageReadModel
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
