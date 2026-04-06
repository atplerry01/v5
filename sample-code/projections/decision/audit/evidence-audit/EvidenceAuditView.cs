namespace Whycespace.Projections.Decision.Audit.EvidenceAudit;

public sealed record EvidenceAuditView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
