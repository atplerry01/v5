using Whycespace.Shared.Contracts.Events.Platform.Routing.RouteDefinition;
using DomainEvents = Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformRouteDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("RouteDefinitionRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.RouteDefinitionRegisteredEvent), typeof(RouteDefinitionRegisteredEventSchema));
        sink.RegisterSchema("RouteDefinitionDeactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.RouteDefinitionDeactivatedEvent), typeof(RouteDefinitionDeactivatedEventSchema));
        sink.RegisterSchema("RouteDefinitionDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.RouteDefinitionDeprecatedEvent), typeof(RouteDefinitionDeprecatedEventSchema));

        sink.RegisterPayloadMapper("RouteDefinitionRegisteredEvent", e =>
        {
            var evt = (DomainEvents.RouteDefinitionRegisteredEvent)e;
            return new RouteDefinitionRegisteredEventSchema(
                evt.RouteDefinitionId.Value,
                evt.RouteName,
                evt.SourceRoute.Classification,
                evt.SourceRoute.Context,
                evt.SourceRoute.Domain,
                evt.DestinationRoute.Classification,
                evt.DestinationRoute.Context,
                evt.DestinationRoute.Domain,
                evt.TransportHint.Value,
                evt.Priority);
        });
        sink.RegisterPayloadMapper("RouteDefinitionDeactivatedEvent", e =>
        {
            var evt = (DomainEvents.RouteDefinitionDeactivatedEvent)e;
            return new RouteDefinitionDeactivatedEventSchema(evt.RouteDefinitionId.Value);
        });
        sink.RegisterPayloadMapper("RouteDefinitionDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.RouteDefinitionDeprecatedEvent)e;
            return new RouteDefinitionDeprecatedEventSchema(evt.RouteDefinitionId.Value);
        });
    }
}
