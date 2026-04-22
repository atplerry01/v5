using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;

public sealed record RouteDefinitionRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] RouteDefinitionId RouteDefinitionId,
    string RouteName,
    DomainRoute SourceRoute,
    DomainRoute DestinationRoute,
    TransportHint TransportHint,
    int Priority,
    Timestamp RegisteredAt) : DomainEvent;
