using Whycespace.Shared.Contracts.Events.Control.Audit.AuditEvent;
using DomainEvents = Whycespace.Domain.ControlSystem.Audit.AuditEvent;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAuditAuditEventSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AuditEventCapturedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditEventCapturedEvent), typeof(AuditEventCapturedEventSchema));
        sink.RegisterSchema("AuditEventSealedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditEventSealedEvent), typeof(AuditEventSealedEventSchema));

        sink.RegisterPayloadMapper("AuditEventCapturedEvent", e =>
        {
            var evt = (DomainEvents.AuditEventCapturedEvent)e;
            return new AuditEventCapturedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ActorId,
                evt.Action,
                evt.Kind.ToString(),
                evt.CorrelationId,
                evt.OccurredAt);
        });
        sink.RegisterPayloadMapper("AuditEventSealedEvent", e =>
        {
            var evt = (DomainEvents.AuditEventSealedEvent)e;
            return new AuditEventSealedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.IntegrityHash);
        });
    }
}
