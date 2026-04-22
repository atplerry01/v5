using Whycespace.Shared.Contracts.Events.Platform.Event.EventSchema;
using DomainEvents = Whycespace.Domain.PlatformSystem.Event.EventSchema;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformEventSchemaSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("EventSchemaRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.EventSchemaRegisteredEvent), typeof(EventSchemaRegisteredEventSchema));
        sink.RegisterSchema("EventSchemaDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.EventSchemaDeprecatedEvent), typeof(EventSchemaDeprecatedEventSchema));

        sink.RegisterPayloadMapper("EventSchemaRegisteredEvent", e =>
        {
            var evt = (DomainEvents.EventSchemaRegisteredEvent)e;
            return new EventSchemaRegisteredEventSchema(evt.EventSchemaId.Value,
                evt.Name.Value, evt.Version.Value, evt.CompatibilityMode.Value);
        });
        sink.RegisterPayloadMapper("EventSchemaDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.EventSchemaDeprecatedEvent)e;
            return new EventSchemaDeprecatedEventSchema(evt.EventSchemaId.Value);
        });
    }
}
