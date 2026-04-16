using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Path;

public sealed record RoutingPathDisabledEvent(
    [property: JsonPropertyName("AggregateId")] PathId PathId,
    Timestamp DisabledAt) : DomainEvent;
