using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Mandate;

public sealed class MandateProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.mandate";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.mandate.created",
        "whyce.decision.governance.mandate.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IMandateViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new MandateReadModel
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
