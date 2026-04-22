using Whycespace.Shared.Contracts.Events.Control.Observability.SystemMetric;
using DomainEvents = Whycespace.Domain.ControlSystem.Observability.SystemMetric;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlObservabilitySystemMetricSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SystemMetricDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemMetricDefinedEvent), typeof(SystemMetricDefinedEventSchema));
        sink.RegisterSchema("SystemMetricDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemMetricDeprecatedEvent), typeof(SystemMetricDeprecatedEventSchema));

        sink.RegisterPayloadMapper("SystemMetricDefinedEvent", e =>
        {
            var evt = (DomainEvents.SystemMetricDefinedEvent)e;
            return new SystemMetricDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.Type.ToString(),
                evt.Unit,
                evt.LabelNames);
        });
        sink.RegisterPayloadMapper("SystemMetricDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.SystemMetricDeprecatedEvent)e;
            return new SystemMetricDeprecatedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
