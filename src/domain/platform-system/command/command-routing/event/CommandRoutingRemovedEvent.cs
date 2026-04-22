using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandRouting;

public sealed record CommandRoutingRemovedEvent(
    [property: JsonPropertyName("AggregateId")] CommandRoutingRuleId CommandRoutingRuleId,
    Timestamp RemovedAt) : DomainEvent;
