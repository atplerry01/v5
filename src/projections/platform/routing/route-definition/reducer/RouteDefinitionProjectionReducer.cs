using Whycespace.Shared.Contracts.Events.Platform.Routing.RouteDefinition;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;

namespace Whycespace.Projections.Platform.Routing.RouteDefinition.Reducer;

public static class RouteDefinitionProjectionReducer
{
    public static RouteDefinitionReadModel Apply(RouteDefinitionReadModel state, RouteDefinitionRegisteredEventSchema e, DateTimeOffset at) =>
        state with
        {
            RouteDefinitionId = e.AggregateId,
            RouteName = e.RouteName,
            SourceClassification = e.SourceClassification,
            SourceContext = e.SourceContext,
            SourceDomain = e.SourceDomain,
            DestinationClassification = e.DestinationClassification,
            DestinationContext = e.DestinationContext,
            DestinationDomain = e.DestinationDomain,
            TransportHint = e.TransportHint,
            Priority = e.Priority,
            Status = "Active",
            LastModifiedAt = at
        };

    public static RouteDefinitionReadModel Apply(RouteDefinitionReadModel state, RouteDefinitionDeactivatedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Inactive", LastModifiedAt = at };

    public static RouteDefinitionReadModel Apply(RouteDefinitionReadModel state, RouteDefinitionDeprecatedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Deprecated", LastModifiedAt = at };
}
