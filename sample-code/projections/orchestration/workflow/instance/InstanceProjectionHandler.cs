using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Instance;

public sealed class InstanceProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.instance";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.instance.created",
        "whyce.orchestration.workflow.instance.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IInstanceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new InstanceReadModel
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
