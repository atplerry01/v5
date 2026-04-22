using Whycespace.Shared.Contracts.Events.Control.Observability.SystemSignal;
using DomainEvents = Whycespace.Domain.ControlSystem.Observability.SystemSignal;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlObservabilitySystemSignalSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SystemSignalDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemSignalDefinedEvent), typeof(SystemSignalDefinedEventSchema));
        sink.RegisterSchema("SystemSignalDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemSignalDeprecatedEvent), typeof(SystemSignalDeprecatedEventSchema));

        sink.RegisterPayloadMapper("SystemSignalDefinedEvent", e =>
        {
            var evt = (DomainEvents.SystemSignalDefinedEvent)e;
            return new SystemSignalDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.Kind.ToString(),
                evt.Source);
        });
        sink.RegisterPayloadMapper("SystemSignalDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.SystemSignalDeprecatedEvent)e;
            return new SystemSignalDeprecatedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
