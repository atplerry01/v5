using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Rule;

public sealed class EnforcementRuleAggregate : AggregateRoot
{
    public RuleId RuleId { get; private set; }
    public RuleCode RuleCode { get; private set; }
    public RuleName RuleName { get; private set; }
    public RuleCategory RuleCategory { get; private set; }
    public RuleScope Scope { get; private set; }
    public RuleSeverity Severity { get; private set; }
    public RuleStatus Status { get; private set; }
    public DocumentRef Description { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    private EnforcementRuleAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static EnforcementRuleAggregate Define(
        RuleId ruleId,
        RuleCode ruleCode,
        RuleName ruleName,
        RuleCategory ruleCategory,
        RuleScope scope,
        RuleSeverity severity,
        DocumentRef description,
        Timestamp createdAt)
    {
        var aggregate = new EnforcementRuleAggregate();
        aggregate.RaiseDomainEvent(new EnforcementRuleDefinedEvent(
            ruleId, ruleCode, ruleName.Value, ruleCategory, scope, severity, description, createdAt));
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
                RuleName = new RuleName(e.RuleName);
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
        if (string.IsNullOrWhiteSpace(RuleName.Value))
            throw new DomainInvariantViolationException("Invariant violated: rule must have a name.");

        if (Description == default)
            throw new DomainInvariantViolationException("Invariant violated: rule must reference a description content aggregate.");
    }
}
