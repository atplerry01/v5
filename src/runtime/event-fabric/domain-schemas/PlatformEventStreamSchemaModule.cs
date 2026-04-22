using Whycespace.Shared.Contracts.Events.Platform.Event.EventStream;
using DomainEvents = Whycespace.Domain.PlatformSystem.Event.EventStream;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformEventStreamSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("EventStreamDeclaredEvent", EventVersion.Default,
            typeof(DomainEvents.EventStreamDeclaredEvent), typeof(EventStreamDeclaredEventSchema));

        sink.RegisterPayloadMapper("EventStreamDeclaredEvent", e =>
        {
            var evt = (DomainEvents.EventStreamDeclaredEvent)e;
            return new EventStreamDeclaredEventSchema(evt.EventStreamId.Value,
                evt.SourceRoute.Classification, evt.SourceRoute.Context, evt.SourceRoute.Domain,
                evt.IncludedEventTypes, evt.OrderingGuarantee.Value);
        });
    }
}
