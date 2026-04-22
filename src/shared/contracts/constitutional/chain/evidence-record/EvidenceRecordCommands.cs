using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;

public sealed record RecordEvidenceCommand(
    Guid EvidenceRecordId,
    Guid CorrelationId,
    Guid AnchorRecordId,
    string EvidenceType,
    string ActorId,
    string SubjectId,
    string PolicyHash,
    DateTimeOffset RecordedAt) : IHasAggregateId
{
    public Guid AggregateId => EvidenceRecordId;
}

public sealed record ArchiveEvidenceCommand(
    Guid EvidenceRecordId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => EvidenceRecordId;
}
