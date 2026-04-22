using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;

public sealed record RegisterDispatchRuleCommand(
    Guid DispatchRuleId,
    string RuleName,
    Guid RouteRef,
    string ConditionType,
    string MatchValue,
    int Priority,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => DispatchRuleId;
}

public sealed record DeactivateDispatchRuleCommand(
    Guid DispatchRuleId,
    DateTimeOffset DeactivatedAt) : IHasAggregateId
{
    public Guid AggregateId => DispatchRuleId;
}
