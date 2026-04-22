using Whycespace.Shared.Contracts.Events.Platform.Routing.RouteDescriptor;
using DomainEvents = Whycespace.Domain.PlatformSystem.Routing.RouteDescriptor;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformRouteDescriptorSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("RouteDescriptorRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.RouteDescriptorRegisteredEvent), typeof(RouteDescriptorRegisteredEventSchema));

        sink.RegisterPayloadMapper("RouteDescriptorRegisteredEvent", e =>
        {
            var evt = (DomainEvents.RouteDescriptorRegisteredEvent)e;
            return new RouteDescriptorRegisteredEventSchema(
                evt.RouteDescriptorId.Value,
                evt.Source.Classification,
                evt.Source.Context,
                evt.Source.Domain,
                evt.Destination.Classification,
                evt.Destination.Context,
                evt.Destination.Domain,
                evt.TransportHint,
                evt.Priority);
        });
    }
}
