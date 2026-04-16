namespace Whycespace.Shared.Contracts.Economic.Compliance.Audit;

public sealed record AuditRecordReadModel
{
    public Guid AuditRecordId { get; init; }
    public string SourceDomain { get; init; } = string.Empty;
    public Guid SourceAggregateId { get; init; }
    public Guid SourceEventId { get; init; }
    public string AuditType { get; init; } = string.Empty;
    public string EvidenceSummary { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
