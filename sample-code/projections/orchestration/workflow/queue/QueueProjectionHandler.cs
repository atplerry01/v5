using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Queue;

public sealed class QueueProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.queue";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.queue.created",
        "whyce.orchestration.workflow.queue.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IQueueViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new QueueReadModel
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
