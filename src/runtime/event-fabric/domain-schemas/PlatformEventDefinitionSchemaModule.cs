using Whycespace.Shared.Contracts.Events.Platform.Event.EventDefinition;
using DomainEvents = Whycespace.Domain.PlatformSystem.Event.EventDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformEventDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("EventDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.EventDefinedEvent), typeof(EventDefinedEventSchema));
        sink.RegisterSchema("EventDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.EventDeprecatedEvent), typeof(EventDefinitionDeprecatedEventSchema));

        sink.RegisterPayloadMapper("EventDefinedEvent", e =>
        {
            var evt = (DomainEvents.EventDefinedEvent)e;
            return new EventDefinedEventSchema(evt.EventDefinitionId.Value,
                evt.TypeName.Value, evt.Version.Value, evt.SchemaId,
                evt.SourceRoute.Classification, evt.SourceRoute.Context, evt.SourceRoute.Domain);
        });
        sink.RegisterPayloadMapper("EventDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.EventDeprecatedEvent)e;
            return new EventDefinitionDeprecatedEventSchema(evt.EventDefinitionId.Value);
        });
    }
}
