using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Rule;

public sealed record EnforcementRuleDefinedEvent(
    RuleId RuleId,
    RuleCode RuleCode,
    string RuleName,
    RuleCategory RuleCategory,
    RuleScope Scope,
    RuleSeverity Severity,
    DocumentRef Description,
    Timestamp CreatedAt) : DomainEvent;
