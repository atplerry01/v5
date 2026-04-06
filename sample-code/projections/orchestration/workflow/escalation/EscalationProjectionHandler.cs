using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Orchestration.Workflow.Escalation;

public sealed class EscalationProjectionHandler
{
    public string ProjectionName => "whyce.orchestration.workflow.escalation";

    public string[] EventTypes =>
    [
        "whyce.orchestration.workflow.escalation.created",
        "whyce.orchestration.workflow.escalation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEscalationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EscalationReadModel
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
