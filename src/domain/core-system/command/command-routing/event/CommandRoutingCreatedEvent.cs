using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public sealed record CommandRoutingDefinedEvent(
    [property: JsonPropertyName("AggregateId")] CommandRoutingId RoutingId,
    RoutingRule Rule) : DomainEvent;
