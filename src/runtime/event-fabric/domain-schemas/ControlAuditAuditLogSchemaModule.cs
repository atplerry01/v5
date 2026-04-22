using Whycespace.Shared.Contracts.Events.Control.Audit.AuditLog;
using DomainEvents = Whycespace.Domain.ControlSystem.Audit.AuditLog;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAuditAuditLogSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AuditEntryRecordedEvent", EventVersion.Default,
            typeof(DomainEvents.AuditEntryRecordedEvent), typeof(AuditEntryRecordedEventSchema));

        sink.RegisterPayloadMapper("AuditEntryRecordedEvent", e =>
        {
            var evt = (DomainEvents.AuditEntryRecordedEvent)e;
            return new AuditEntryRecordedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ActorId,
                evt.Action,
                evt.Resource,
                evt.Classification.ToString(),
                evt.OccurredAt,
                evt.DecisionId);
        });
    }
}
