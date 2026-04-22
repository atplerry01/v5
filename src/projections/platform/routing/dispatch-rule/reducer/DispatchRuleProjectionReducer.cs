using Whycespace.Shared.Contracts.Events.Platform.Routing.DispatchRule;
using Whycespace.Shared.Contracts.Platform.Routing.DispatchRule;

namespace Whycespace.Projections.Platform.Routing.DispatchRule.Reducer;

public static class DispatchRuleProjectionReducer
{
    public static DispatchRuleReadModel Apply(DispatchRuleReadModel state, DispatchRuleRegisteredEventSchema e, DateTimeOffset at) =>
        state with
        {
            DispatchRuleId = e.AggregateId,
            RuleName = e.RuleName,
            RouteRef = e.RouteRef,
            ConditionType = e.ConditionType,
            MatchValue = e.MatchValue,
            Priority = e.Priority,
            Status = "Active",
            LastModifiedAt = at
        };

    public static DispatchRuleReadModel Apply(DispatchRuleReadModel state, DispatchRuleDeactivatedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Inactive", LastModifiedAt = at };
}
