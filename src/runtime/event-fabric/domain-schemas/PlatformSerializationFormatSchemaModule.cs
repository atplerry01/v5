using Whycespace.Shared.Contracts.Events.Platform.Schema.Serialization;
using DomainEvents = Whycespace.Domain.PlatformSystem.Schema.Serialization;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformSerializationFormatSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SerializationFormatRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.SerializationFormatRegisteredEvent), typeof(SerializationFormatRegisteredEventSchema));
        sink.RegisterSchema("SerializationFormatDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.SerializationFormatDeprecatedEvent), typeof(SerializationFormatDeprecatedEventSchema));

        sink.RegisterPayloadMapper("SerializationFormatRegisteredEvent", e =>
        {
            var evt = (DomainEvents.SerializationFormatRegisteredEvent)e;
            return new SerializationFormatRegisteredEventSchema(
                evt.SerializationFormatId.Value,
                evt.FormatName,
                evt.Encoding.Value,
                evt.SchemaRef,
                evt.RoundTripGuarantee.Value,
                evt.FormatVersion);
        });
        sink.RegisterPayloadMapper("SerializationFormatDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.SerializationFormatDeprecatedEvent)e;
            return new SerializationFormatDeprecatedEventSchema(evt.SerializationFormatId.Value);
        });
    }
}
