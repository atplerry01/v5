using Whycespace.Shared.Contracts.Control.Audit.AuditLog;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditLog;

namespace Whycespace.Projections.Control.Audit.AuditLog.Reducer;

public static class AuditLogProjectionReducer
{
    public static AuditLogReadModel Apply(AuditLogReadModel state, AuditEntryRecordedEventSchema e) =>
        state with
        {
            AuditLogId     = e.AggregateId,
            ActorId        = e.ActorId,
            Action         = e.Action,
            Resource       = e.Resource,
            Classification = e.Classification,
            OccurredAt     = e.OccurredAt,
            DecisionId     = e.DecisionId
        };
}
