using Whycespace.Shared.Contracts.Events.Platform.Event.EventMetadata;
using DomainEvents = Whycespace.Domain.PlatformSystem.Event.EventMetadata;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformEventMetadataSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("EventMetadataAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.EventMetadataAttachedEvent), typeof(EventMetadataAttachedEventSchema));

        sink.RegisterPayloadMapper("EventMetadataAttachedEvent", e =>
        {
            var evt = (DomainEvents.EventMetadataAttachedEvent)e;
            return new EventMetadataAttachedEventSchema(evt.EventMetadataId.Value,
                evt.EnvelopeRef.Value, evt.ExecutionHash.Value, evt.PolicyDecisionHash.Value,
                evt.ActorId.Value, evt.TraceId.Value, evt.SpanId.Value, evt.IssuedAt.Value);
        });
    }
}
