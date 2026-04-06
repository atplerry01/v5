using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Audit.AuditLog;

public sealed class AuditLogProjectionHandler
{
    public string ProjectionName => "whyce.decision.audit.audit-log";

    public string[] EventTypes =>
    [
        "whyce.decision.audit.audit-log.created",
        "whyce.decision.audit.audit-log.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAuditLogViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AuditLogReadModel
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
