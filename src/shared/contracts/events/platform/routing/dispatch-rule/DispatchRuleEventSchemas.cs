namespace Whycespace.Shared.Contracts.Events.Platform.Routing.DispatchRule;

public sealed record DispatchRuleRegisteredEventSchema(
    Guid AggregateId,
    string RuleName,
    Guid RouteRef,
    string ConditionType,
    string MatchValue,
    int Priority);

public sealed record DispatchRuleDeactivatedEventSchema(Guid AggregateId);
