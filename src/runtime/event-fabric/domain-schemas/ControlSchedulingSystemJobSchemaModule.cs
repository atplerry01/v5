using Whycespace.Shared.Contracts.Events.Control.Scheduling.SystemJob;
using DomainEvents = Whycespace.Domain.ControlSystem.Scheduling.SystemJob;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSchedulingSystemJobSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SystemJobDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemJobDefinedEvent), typeof(SystemJobDefinedEventSchema));
        sink.RegisterSchema("SystemJobDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemJobDeprecatedEvent), typeof(SystemJobDeprecatedEventSchema));

        sink.RegisterPayloadMapper("SystemJobDefinedEvent", e =>
        {
            var evt = (DomainEvents.SystemJobDefinedEvent)e;
            return new SystemJobDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.Category.ToString(),
                evt.Timeout);
        });
        sink.RegisterPayloadMapper("SystemJobDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.SystemJobDeprecatedEvent)e;
            return new SystemJobDeprecatedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
