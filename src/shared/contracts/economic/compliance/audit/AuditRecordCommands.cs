using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Compliance.Audit;

public sealed record CreateAuditRecordCommand(
    Guid AuditRecordId,
    string SourceDomain,
    Guid SourceAggregateId,
    Guid SourceEventId,
    string AuditType,
    string EvidenceSummary,
    DateTimeOffset RecordedAt) : IHasAggregateId
{
    public Guid AggregateId => AuditRecordId;
}

public sealed record FinalizeAuditRecordCommand(
    Guid AuditRecordId,
    DateTimeOffset FinalizedAt) : IHasAggregateId
{
    public Guid AggregateId => AuditRecordId;
}
