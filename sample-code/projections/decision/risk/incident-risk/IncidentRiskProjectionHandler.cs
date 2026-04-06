using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.IncidentRisk;

public sealed class IncidentRiskProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.incident-risk";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.incident-risk.created",
        "whyce.decision.risk.incident-risk.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIncidentRiskViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new IncidentRiskReadModel
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
