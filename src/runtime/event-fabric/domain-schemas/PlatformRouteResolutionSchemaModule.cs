using Whycespace.Shared.Contracts.Events.Platform.Routing.RouteResolution;
using DomainEvents = Whycespace.Domain.PlatformSystem.Routing.RouteResolution;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformRouteResolutionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("RouteResolvedEvent", EventVersion.Default,
            typeof(DomainEvents.RouteResolvedEvent), typeof(RouteResolvedEventSchema));
        sink.RegisterSchema("RouteResolutionFailedEvent", EventVersion.Default,
            typeof(DomainEvents.RouteResolutionFailedEvent), typeof(RouteResolutionFailedEventSchema));

        sink.RegisterPayloadMapper("RouteResolvedEvent", e =>
        {
            var evt = (DomainEvents.RouteResolvedEvent)e;
            return new RouteResolvedEventSchema(
                evt.ResolutionId.Value,
                evt.SourceRoute.Classification,
                evt.SourceRoute.Context,
                evt.SourceRoute.Domain,
                evt.MessageType,
                evt.ResolvedRouteRef,
                evt.Strategy.Value,
                evt.DispatchRulesEvaluated);
        });
        sink.RegisterPayloadMapper("RouteResolutionFailedEvent", e =>
        {
            var evt = (DomainEvents.RouteResolutionFailedEvent)e;
            return new RouteResolutionFailedEventSchema(
                evt.ResolutionId.Value,
                evt.SourceRoute.Classification,
                evt.SourceRoute.Context,
                evt.SourceRoute.Domain,
                evt.MessageType,
                evt.DispatchRulesEvaluated,
                evt.FailureReason);
        });
    }
}
