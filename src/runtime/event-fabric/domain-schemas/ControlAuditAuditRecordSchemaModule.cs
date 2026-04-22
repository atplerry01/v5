using Whycespace.Shared.Contracts.Events.Control.Audit.AuditRecord;
using DomainEvents = Whycespace.Domain.ControlSystem.Audit.AuditRecord;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAuditAuditRecordSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AuditRecordRaisedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditRecordRaisedEvent), typeof(AuditRecordRaisedEventSchema));
        sink.RegisterSchema("AuditRecordResolvedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditRecordResolvedEvent), typeof(AuditRecordResolvedEventSchema));

        sink.RegisterPayloadMapper("AuditRecordRaisedEvent", e =>
        {
            var evt = (DomainEvents.AuditRecordRaisedEvent)e;
            return new AuditRecordRaisedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.AuditLogEntryIds,
                evt.Description,
                evt.Severity.ToString(),
                evt.RaisedAt);
        });
        sink.RegisterPayloadMapper("AuditRecordResolvedEvent", e =>
        {
            var evt = (DomainEvents.AuditRecordResolvedEvent)e;
            return new AuditRecordResolvedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ResolvedAt);
        });
    }
}
