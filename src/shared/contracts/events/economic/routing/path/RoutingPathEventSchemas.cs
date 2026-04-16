namespace Whycespace.Shared.Contracts.Events.Economic.Routing.Path;

public sealed record RoutingPathDefinedEventSchema(
    Guid AggregateId,
    string PathType,
    string Conditions,
    int Priority);

public sealed record RoutingPathActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record RoutingPathDisabledEventSchema(
    Guid AggregateId,
    DateTimeOffset DisabledAt);
