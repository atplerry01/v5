using Whycespace.Shared.Contracts.Control.Audit.AuditEvent;
using Whycespace.Shared.Contracts.Events.Control.Audit.AuditEvent;

namespace Whycespace.Projections.Control.Audit.AuditEvent.Reducer;

public static class AuditEventProjectionReducer
{
    public static AuditEventReadModel Apply(AuditEventReadModel state, AuditEventCapturedEventSchema e) =>
        state with
        {
            AuditEventId  = e.AggregateId,
            ActorId       = e.ActorId,
            Action        = e.Action,
            Kind          = e.Kind,
            CorrelationId = e.CorrelationId,
            OccurredAt    = e.OccurredAt,
            IsSealed      = false
        };

    public static AuditEventReadModel Apply(AuditEventReadModel state, AuditEventSealedEventSchema e) =>
        state with
        {
            IsSealed      = true,
            IntegrityHash = e.IntegrityHash
        };
}
