using Whycespace.Shared.Contracts.Events.Control.Observability.SystemHealth;
using DomainEvents = Whycespace.Domain.ControlSystem.Observability.SystemHealth;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlObservabilitySystemHealthSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SystemHealthEvaluatedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemHealthEvaluatedEvent), typeof(SystemHealthEvaluatedEventSchema));
        sink.RegisterSchema("SystemHealthDegradedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemHealthDegradedEvent), typeof(SystemHealthDegradedEventSchema));
        sink.RegisterSchema("SystemHealthRestoredEvent", EventVersion.Default,
            typeof(DomainEvents.SystemHealthRestoredEvent), typeof(SystemHealthRestoredEventSchema));

        sink.RegisterPayloadMapper("SystemHealthEvaluatedEvent", e =>
        {
            var evt = (DomainEvents.SystemHealthEvaluatedEvent)e;
            return new SystemHealthEvaluatedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.ComponentName,
                evt.Status.ToString(),
                evt.EvaluatedAt);
        });
        sink.RegisterPayloadMapper("SystemHealthDegradedEvent", e =>
        {
            var evt = (DomainEvents.SystemHealthDegradedEvent)e;
            return new SystemHealthDegradedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.NewStatus.ToString(),
                evt.Reason,
                evt.OccurredAt);
        });
        sink.RegisterPayloadMapper("SystemHealthRestoredEvent", e =>
        {
            var evt = (DomainEvents.SystemHealthRestoredEvent)e;
            return new SystemHealthRestoredEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.RestoredAt);
        });
    }
}
