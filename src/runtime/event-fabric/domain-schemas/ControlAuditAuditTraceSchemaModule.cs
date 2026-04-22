using Whycespace.Shared.Contracts.Events.Control.Audit.AuditTrace;
using DomainEvents = Whycespace.Domain.ControlSystem.Audit.AuditTrace;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAuditAuditTraceSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AuditTraceOpenedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditTraceOpenedEvent), typeof(AuditTraceOpenedEventSchema));
        sink.RegisterSchema("AuditTraceEventLinkedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditTraceEventLinkedEvent), typeof(AuditTraceEventLinkedEventSchema));
        sink.RegisterSchema("AuditTraceClosedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditTraceClosedEvent), typeof(AuditTraceClosedEventSchema));

        sink.RegisterPayloadMapper("AuditTraceOpenedEvent", e =>
        {
            var evt = (DomainEvents.AuditTraceOpenedEvent)e;
            return new AuditTraceOpenedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.CorrelationId,
                evt.OpenedAt);
        });
        sink.RegisterPayloadMapper("AuditTraceEventLinkedEvent", e =>
        {
            var evt = (DomainEvents.AuditTraceEventLinkedEvent)e;
            return new AuditTraceEventLinkedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.AuditEventId);
        });
        sink.RegisterPayloadMapper("AuditTraceClosedEvent", e =>
        {
            var evt = (DomainEvents.AuditTraceClosedEvent)e;
            return new AuditTraceClosedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ClosedAt);
        });
    }
}
