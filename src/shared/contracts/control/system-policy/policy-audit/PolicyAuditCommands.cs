using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyAudit;

public sealed record RecordPolicyAuditEntryCommand(
    Guid AuditId,
    string PolicyId,
    string ActorId,
    string Action,
    string Category,
    string DecisionHash,
    string CorrelationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => AuditId;
}

public sealed record ReviewPolicyAuditEntryCommand(
    Guid AuditId,
    string ReviewerId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => AuditId;
}
