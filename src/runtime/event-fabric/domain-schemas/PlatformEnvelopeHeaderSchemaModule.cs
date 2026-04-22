using Whycespace.Shared.Contracts.Events.Platform.Envelope.Header;
using DomainEvents = Whycespace.Domain.PlatformSystem.Envelope.Header;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformEnvelopeHeaderSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("HeaderSchemaRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.HeaderSchemaRegisteredEvent), typeof(HeaderSchemaRegisteredEventSchema));
        sink.RegisterSchema("HeaderSchemaDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.HeaderSchemaDeprecatedEvent), typeof(HeaderSchemaDeprecatedEventSchema));

        sink.RegisterPayloadMapper("HeaderSchemaRegisteredEvent", e =>
        {
            var evt = (DomainEvents.HeaderSchemaRegisteredEvent)e;
            return new HeaderSchemaRegisteredEventSchema(evt.HeaderSchemaId.Value,
                evt.HeaderKind.Value, evt.SchemaVersion, evt.RequiredFields);
        });
        sink.RegisterPayloadMapper("HeaderSchemaDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.HeaderSchemaDeprecatedEvent)e;
            return new HeaderSchemaDeprecatedEventSchema(evt.HeaderSchemaId.Value);
        });
    }
}
