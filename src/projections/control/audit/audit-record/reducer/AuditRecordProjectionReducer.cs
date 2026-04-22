using Whycespace.Shared.Contracts.Control.Audit.AuditRecord;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditRecord;

namespace Whycespace.Projections.Control.Audit.AuditRecord.Reducer;

public static class AuditRecordProjectionReducer
{
    public static AuditRecordReadModel Apply(AuditRecordReadModel state, AuditRecordRaisedEventSchema e) =>
        state with
        {
            RecordId          = e.AggregateId,
            AuditLogEntryIds  = e.AuditLogEntryIds,
            Description       = e.Description,
            Severity          = e.Severity,
            Status            = "Open",
            RaisedAt          = e.RaisedAt
        };

    public static AuditRecordReadModel Apply(AuditRecordReadModel state, AuditRecordResolvedEventSchema e) =>
        state with
        {
            Status     = "Resolved",
            ResolvedAt = e.ResolvedAt
        };
}
