using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Transition;

public sealed class TransitionProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.transition";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.transition.created",
        "whyce.orchestration.workflow.transition.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITransitionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TransitionReadModel
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
