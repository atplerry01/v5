using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Lifecycle;

public sealed class LifecycleProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.lifecycle";

    public string[] EventTypes =>
    [
        "whyce.business.execution.lifecycle.created",
        "whyce.business.execution.lifecycle.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILifecycleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new LifecycleReadModel
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
