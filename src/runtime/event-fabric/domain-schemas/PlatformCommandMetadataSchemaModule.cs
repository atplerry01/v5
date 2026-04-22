using Whycespace.Shared.Contracts.Events.Platform.Command.CommandMetadata;
using DomainEvents = Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformCommandMetadataSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("CommandMetadataAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.CommandMetadataAttachedEvent), typeof(CommandMetadataAttachedEventSchema));

        sink.RegisterPayloadMapper("CommandMetadataAttachedEvent", e =>
        {
            var evt = (DomainEvents.CommandMetadataAttachedEvent)e;
            return new CommandMetadataAttachedEventSchema(evt.CommandMetadataId.Value,
                evt.EnvelopeRef, evt.ActorId.Value, evt.TraceId.Value, evt.SpanId.Value,
                evt.PolicyContextRef.PolicyId, evt.PolicyContextRef.PolicyVersion,
                evt.TrustScore.Value, evt.IssuedAt.Value);
        });
    }
}
