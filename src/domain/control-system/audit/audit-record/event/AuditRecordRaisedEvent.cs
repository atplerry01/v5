using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Audit.AuditRecord;

public sealed record AuditRecordRaisedEvent(
    AuditRecordId Id,
    IReadOnlyList<string> AuditLogEntryIds,
    string Description,
    AuditRecordSeverity Severity,
    DateTimeOffset RaisedAt) : DomainEvent;
