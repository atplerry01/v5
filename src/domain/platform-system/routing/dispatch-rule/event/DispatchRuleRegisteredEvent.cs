using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

public sealed record DispatchRuleRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] DispatchRuleId DispatchRuleId,
    string RuleName,
    Guid RouteRef,
    DispatchCondition Condition,
    int Priority,
    Timestamp RegisteredAt) : DomainEvent;
