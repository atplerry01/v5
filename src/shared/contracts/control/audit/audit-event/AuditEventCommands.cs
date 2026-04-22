using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Audit.AuditEvent;

public sealed record CaptureAuditEventCommand(
    Guid AuditEventId,
    string ActorId,
    string Action,
    string Kind,
    string CorrelationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => AuditEventId;
}

public sealed record SealAuditEventCommand(
    Guid AuditEventId,
    string IntegrityHash) : IHasAggregateId
{
    public Guid AggregateId => AuditEventId;
}
