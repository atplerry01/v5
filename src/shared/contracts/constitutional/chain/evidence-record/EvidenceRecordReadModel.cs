namespace Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;

public sealed record EvidenceRecordReadModel
{
    public Guid EvidenceRecordId { get; init; }
    public Guid CorrelationId { get; init; }
    public Guid AnchorRecordId { get; init; }
    public string EvidenceType { get; init; } = string.Empty;
    public string ActorId { get; init; } = string.Empty;
    public string SubjectId { get; init; } = string.Empty;
    public string PolicyHash { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
    public DateTimeOffset? ArchivedAt { get; init; }
}
