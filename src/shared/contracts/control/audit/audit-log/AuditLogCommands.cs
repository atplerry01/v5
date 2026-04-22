using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Audit.AuditLog;

public sealed record RecordAuditLogEntryCommand(
    Guid AuditLogId,
    string ActorId,
    string Action,
    string Resource,
    string Classification,
    DateTimeOffset OccurredAt,
    string? DecisionId = null) : IHasAggregateId
{
    public Guid AggregateId => AuditLogId;
}
