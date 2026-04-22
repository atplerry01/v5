using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;

public sealed record RouteDefinitionDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] RouteDefinitionId RouteDefinitionId,
    Timestamp DeprecatedAt) : DomainEvent;
