namespace Whycespace.Shared.Contracts.Events.Control.Audit.AuditRecord;

public sealed record AuditRecordRaisedEventSchema(
    Guid AggregateId,
    IReadOnlyList<string> AuditLogEntryIds,
    string Description,
    string Severity,
    DateTimeOffset RaisedAt);

public sealed record AuditRecordResolvedEventSchema(
    Guid AggregateId,
    DateTimeOffset ResolvedAt);
