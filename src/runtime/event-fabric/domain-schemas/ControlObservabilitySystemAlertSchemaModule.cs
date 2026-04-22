using Whycespace.Shared.Contracts.Events.Control.Observability.SystemAlert;
using DomainEvents = Whycespace.Domain.ControlSystem.Observability.SystemAlert;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlObservabilitySystemAlertSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SystemAlertDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemAlertDefinedEvent), typeof(SystemAlertDefinedEventSchema));
        sink.RegisterSchema("SystemAlertRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.SystemAlertRetiredEvent), typeof(SystemAlertRetiredEventSchema));

        sink.RegisterPayloadMapper("SystemAlertDefinedEvent", e =>
        {
            var evt = (DomainEvents.SystemAlertDefinedEvent)e;
            return new SystemAlertDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.MetricDefinitionId,
                evt.ConditionExpression,
                evt.Severity.ToString());
        });
        sink.RegisterPayloadMapper("SystemAlertRetiredEvent", e =>
        {
            var evt = (DomainEvents.SystemAlertRetiredEvent)e;
            return new SystemAlertRetiredEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
