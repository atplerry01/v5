using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Enforcement.Rule;

public sealed record DefineEnforcementRuleCommand(
    Guid RuleId,
    string RuleCode,
    string RuleName,
    string RuleCategory,
    string Scope,
    string Severity,
    string Description,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => RuleId;
}

public sealed record ActivateEnforcementRuleCommand(Guid RuleId) : IHasAggregateId
{
    public Guid AggregateId => RuleId;
}

public sealed record DisableEnforcementRuleCommand(Guid RuleId) : IHasAggregateId
{
    public Guid AggregateId => RuleId;
}

public sealed record RetireEnforcementRuleCommand(Guid RuleId) : IHasAggregateId
{
    public Guid AggregateId => RuleId;
}
