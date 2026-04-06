using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Audit.AuditCase;

public sealed class AuditCaseProjectionHandler
{
    public string ProjectionName => "whyce.decision.audit.audit-case";

    public string[] EventTypes =>
    [
        "whyce.decision.audit.audit-case.created",
        "whyce.decision.audit.audit-case.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAuditCaseViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AuditCaseReadModel
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
