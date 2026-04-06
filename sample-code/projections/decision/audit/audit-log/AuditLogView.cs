namespace Whycespace.Projections.Decision.Audit.AuditLog;

public sealed record AuditLogView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
