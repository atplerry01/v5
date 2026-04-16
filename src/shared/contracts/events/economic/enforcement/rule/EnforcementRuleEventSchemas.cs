namespace Whycespace.Shared.Contracts.Events.Economic.Enforcement.Rule;

public sealed record EnforcementRuleDefinedEventSchema(
    Guid AggregateId,
    string RuleCode,
    string RuleName,
    string RuleCategory,
    string Scope,
    string Severity,
    string Description,
    DateTimeOffset CreatedAt);

public sealed record EnforcementRuleActivatedEventSchema(Guid AggregateId);

public sealed record EnforcementRuleDisabledEventSchema(Guid AggregateId);

public sealed record EnforcementRuleRetiredEventSchema(Guid AggregateId);
