namespace Whycespace.Shared.Contracts.Economic.Enforcement.Rule;

public sealed record GetEnforcementRuleByIdQuery(Guid RuleId);

public sealed record ListEnforcementRulesByScopeQuery(string Scope);
