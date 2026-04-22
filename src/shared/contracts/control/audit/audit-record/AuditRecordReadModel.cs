namespace Whycespace.Shared.Contracts.Control.Audit.AuditRecord;

public sealed record AuditRecordReadModel
{
    public Guid RecordId { get; init; }
    public IReadOnlyList<string> AuditLogEntryIds { get; init; } = [];
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset RaisedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
}
