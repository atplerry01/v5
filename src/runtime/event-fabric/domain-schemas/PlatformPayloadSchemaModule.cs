using Whycespace.Shared.Contracts.Events.Platform.Envelope.Payload;
using DomainEvents = Whycespace.Domain.PlatformSystem.Envelope.Payload;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformPayloadSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PayloadSchemaRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.PayloadSchemaRegisteredEvent), typeof(PayloadSchemaRegisteredEventSchema));
        sink.RegisterSchema("PayloadSchemaDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.PayloadSchemaDeprecatedEvent), typeof(PayloadSchemaDeprecatedEventSchema));

        sink.RegisterPayloadMapper("PayloadSchemaRegisteredEvent", e =>
        {
            var evt = (DomainEvents.PayloadSchemaRegisteredEvent)e;
            return new PayloadSchemaRegisteredEventSchema(evt.PayloadSchemaId.Value,
                evt.TypeRef, evt.Encoding.Value, evt.SchemaRef,
                evt.SchemaContractVersion, evt.MaxSizeBytes);
        });
        sink.RegisterPayloadMapper("PayloadSchemaDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.PayloadSchemaDeprecatedEvent)e;
            return new PayloadSchemaDeprecatedEventSchema(evt.PayloadSchemaId.Value);
        });
    }
}
