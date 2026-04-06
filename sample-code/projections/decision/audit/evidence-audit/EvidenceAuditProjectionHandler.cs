using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Audit.EvidenceAudit;

public sealed class EvidenceAuditProjectionHandler
{
    public string ProjectionName => "whyce.decision.audit.evidence-audit";

    public string[] EventTypes =>
    [
        "whyce.decision.audit.evidence-audit.created",
        "whyce.decision.audit.evidence-audit.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEvidenceAuditViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EvidenceAuditReadModel
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
