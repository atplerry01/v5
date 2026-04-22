using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Audit.AuditRecord;

public sealed record RaiseAuditRecordCommand(
    Guid RecordId,
    IReadOnlyList<string> AuditLogEntryIds,
    string Description,
    string Severity,
    DateTimeOffset RaisedAt) : IHasAggregateId
{
    public Guid AggregateId => RecordId;
}

public sealed record ResolveAuditRecordCommand(
    Guid RecordId,
    DateTimeOffset ResolvedAt) : IHasAggregateId
{
    public Guid AggregateId => RecordId;
}
