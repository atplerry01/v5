using Whycespace.Shared.Contracts.Events.Control.Audit.AuditQuery;
using DomainEvents = Whycespace.Domain.ControlSystem.Audit.AuditQuery;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAuditAuditQuerySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AuditQueryIssuedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditQueryIssuedEvent), typeof(AuditQueryIssuedEventSchema));
        sink.RegisterSchema("AuditQueryCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditQueryCompletedEvent), typeof(AuditQueryCompletedEventSchema));
        sink.RegisterSchema("AuditQueryExpiredEvent", EventVersion.Default,
            typeof(DomainEvents.AuditQueryExpiredEvent), typeof(AuditQueryExpiredEventSchema));

        sink.RegisterPayloadMapper("AuditQueryIssuedEvent", e =>
        {
            var evt = (DomainEvents.AuditQueryIssuedEvent)e;
            return new AuditQueryIssuedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.IssuedBy,
                evt.TimeRange.From,
                evt.TimeRange.To,
                evt.CorrelationFilter,
                evt.ActorFilter);
        });
        sink.RegisterPayloadMapper("AuditQueryCompletedEvent", e =>
        {
            var evt = (DomainEvents.AuditQueryCompletedEvent)e;
            return new AuditQueryCompletedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ResultCount);
        });
        sink.RegisterPayloadMapper("AuditQueryExpiredEvent", e =>
        {
            var evt = (DomainEvents.AuditQueryExpiredEvent)e;
            return new AuditQueryExpiredEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
