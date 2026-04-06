using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Checkpoint;

public sealed class CheckpointProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.checkpoint";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.checkpoint.created",
        "whyce.orchestration.workflow.checkpoint.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICheckpointViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CheckpointReadModel
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
