using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;

public sealed record RegisterRouteDefinitionCommand(
    Guid RouteDefinitionId,
    string RouteName,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string DestinationClassification,
    string DestinationContext,
    string DestinationDomain,
    string TransportHint,
    int Priority,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => RouteDefinitionId;
}

public sealed record DeactivateRouteDefinitionCommand(
    Guid RouteDefinitionId,
    DateTimeOffset DeactivatedAt) : IHasAggregateId
{
    public Guid AggregateId => RouteDefinitionId;
}

public sealed record DeprecateRouteDefinitionCommand(
    Guid RouteDefinitionId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => RouteDefinitionId;
}
