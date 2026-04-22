using Whycespace.Shared.Contracts.Events.Platform.Envelope.Metadata;
using DomainEvents = Whycespace.Domain.PlatformSystem.Envelope.Metadata;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformEnvelopeMetadataSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("MessageMetadataSchemaRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.MessageMetadataSchemaRegisteredEvent), typeof(MessageMetadataSchemaRegisteredEventSchema));

        sink.RegisterPayloadMapper("MessageMetadataSchemaRegisteredEvent", e =>
        {
            var evt = (DomainEvents.MessageMetadataSchemaRegisteredEvent)e;
            return new MessageMetadataSchemaRegisteredEventSchema(evt.MetadataSchemaId.Value,
                evt.SchemaVersion, evt.RequiredFields, evt.OptionalFields);
        });
    }
}
