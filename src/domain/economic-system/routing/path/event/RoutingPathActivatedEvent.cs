using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Routing.Path;

public sealed record RoutingPathActivatedEvent(
    [property: JsonPropertyName("AggregateId")] PathId PathId,
    Timestamp ActivatedAt) : DomainEvent;
