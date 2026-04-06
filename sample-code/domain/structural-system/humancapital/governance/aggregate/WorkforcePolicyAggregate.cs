using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Governance;

public sealed class WorkforcePolicyAggregate : AggregateRoot
{
    public PolicyId PolicyId { get; private set; } = new(Guid.Empty);
    public GovernanceRule? Rule { get; private set; }
    public bool IsEnforced { get; private set; }

    public static WorkforcePolicyAggregate Create(Guid policyId, Guid ruleId, string ruleName, string description)
    {
        var policy = new WorkforcePolicyAggregate();
        policy.Rule = new GovernanceRule
        {
            Id = ruleId,
            RuleName = ruleName,
            Description = description
        };
        policy.Apply(new PolicyCreatedEvent(policyId, ruleName));
        return policy;
    }

    public void Update(string newRuleName)
    {
        Apply(new PolicyUpdatedEvent(PolicyId.Value, newRuleName));
    }

    public void Enforce()
    {
        if (IsEnforced)
            throw new DomainException(GovernanceErrors.PolicyViolation, "Policy is already enforced.");

        Apply(new PolicyEnforcedEvent(PolicyId.Value));
    }

    private void Apply(PolicyCreatedEvent e)
    {
        Id = e.PolicyId;
        PolicyId = new PolicyId(e.PolicyId);
        IsEnforced = false;
        RaiseDomainEvent(e);
    }

    private void Apply(PolicyUpdatedEvent e)
    {
        if (Rule != null)
        {
            Rule = new GovernanceRule
            {
                Id = Rule.Id,
                RuleName = e.NewRuleName,
                Description = Rule.Description
            };
        }
        RaiseDomainEvent(e);
    }

    private void Apply(PolicyEnforcedEvent e)
    {
        IsEnforced = true;
        RaiseDomainEvent(e);
    }
}
