using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Rule;

public sealed class EnforcementRuleAggregate : AggregateRoot
{
    public RuleId RuleId { get; private set; }
    public RuleCode RuleCode { get; private set; }
    public string RuleName { get; private set; } = string.Empty;
    public RuleCategory RuleCategory { get; private set; }
    public RuleScope Scope { get; private set; }
    public RuleSeverity Severity { get; private set; }
    public RuleStatus Status { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Timestamp CreatedAt { get; private set; }

    private EnforcementRuleAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static EnforcementRuleAggregate Define(
        RuleId ruleId,
        RuleCode ruleCode,
        string ruleName,
        RuleCategory ruleCategory,
        RuleScope scope,
        RuleSeverity severity,
        string description,
        Timestamp createdAt)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name cannot be empty.", nameof(ruleName));

        var aggregate = new EnforcementRuleAggregate();
        aggregate.RaiseDomainEvent(new EnforcementRuleDefinedEvent(
            ruleId, ruleCode, ruleName, ruleCategory, scope, severity, description, createdAt));
        return aggregate;
    }

    // ── Behaviors ────────────────────────────────────────────────

    public void Activate()
    {
        if (Status == RuleStatus.Retired)
            throw EnforcementRuleErrors.InvalidStateTransition();

        RaiseDomainEvent(new EnforcementRuleActivatedEvent(RuleId));
    }

    public void Disable()
    {
        if (Status != RuleStatus.Active)
            throw EnforcementRuleErrors.InvalidStateTransition();

        RaiseDomainEvent(new EnforcementRuleDisabledEvent(RuleId));
    }

    public void Retire()
    {
        if (Status == RuleStatus.Retired)
            throw EnforcementRuleErrors.InvalidStateTransition();

        RaiseDomainEvent(new EnforcementRuleRetiredEvent(RuleId));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EnforcementRuleDefinedEvent e:
                RuleId = e.RuleId;
                RuleCode = e.RuleCode;
                RuleName = e.RuleName;
                RuleCategory = e.RuleCategory;
                Scope = e.Scope;
                Severity = e.Severity;
                Description = e.Description;
                Status = RuleStatus.Active;
                CreatedAt = e.CreatedAt;
                break;

            case EnforcementRuleActivatedEvent:
                Status = RuleStatus.Active;
                break;

            case EnforcementRuleDisabledEvent:
                Status = RuleStatus.Disabled;
                break;

            case EnforcementRuleRetiredEvent:
                Status = RuleStatus.Retired;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (string.IsNullOrWhiteSpace(RuleName))
            throw new DomainInvariantViolationException("Invariant violated: rule must have a name.");
    }
}
