using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

public sealed record DispatchRuleDeactivatedEvent(
    [property: JsonPropertyName("AggregateId")] DispatchRuleId DispatchRuleId,
    Timestamp DeactivatedAt) : DomainEvent;
