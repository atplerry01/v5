using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Rule;

namespace Whycespace.Projections.Economic.Enforcement.Rule.Reducer;

public static class EnforcementRuleProjectionReducer
{
    public static EnforcementRuleReadModel Apply(EnforcementRuleReadModel state, EnforcementRuleDefinedEventSchema e) =>
        state with
        {
            RuleId = e.AggregateId,
            RuleCode = e.RuleCode,
            RuleName = e.RuleName,
            RuleCategory = e.RuleCategory,
            Scope = e.Scope,
            Severity = e.Severity,
            Status = "Active",
            Description = e.Description,
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt
        };

    public static EnforcementRuleReadModel Apply(EnforcementRuleReadModel state, EnforcementRuleActivatedEventSchema e) =>
        state with { RuleId = e.AggregateId, Status = "Active" };

    public static EnforcementRuleReadModel Apply(EnforcementRuleReadModel state, EnforcementRuleDisabledEventSchema e) =>
        state with { RuleId = e.AggregateId, Status = "Disabled" };

    public static EnforcementRuleReadModel Apply(EnforcementRuleReadModel state, EnforcementRuleRetiredEventSchema e) =>
        state with { RuleId = e.AggregateId, Status = "Retired" };
}
