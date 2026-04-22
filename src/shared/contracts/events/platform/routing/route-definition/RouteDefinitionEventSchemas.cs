namespace Whycespace.Shared.Contracts.Events.Platform.Routing.RouteDefinition;

public sealed record RouteDefinitionRegisteredEventSchema(
    Guid AggregateId,
    string RouteName,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string DestinationClassification,
    string DestinationContext,
    string DestinationDomain,
    string TransportHint,
    int Priority);

public sealed record RouteDefinitionDeactivatedEventSchema(Guid AggregateId);

public sealed record RouteDefinitionDeprecatedEventSchema(Guid AggregateId);
