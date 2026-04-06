namespace Whycespace.Projections.Decision.Audit.AuditCase;

public sealed record AuditCaseView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
