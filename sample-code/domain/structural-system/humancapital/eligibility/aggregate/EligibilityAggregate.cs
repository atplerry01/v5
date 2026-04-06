using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Eligibility;

public sealed class EligibilityAggregate : AggregateRoot
{
    public EligibilityStatus Status { get; private set; } = EligibilityStatus.Pending;
    public EligibilityRule? Rule { get; private set; }
    public bool IsEligible { get; private set; }

    public static EligibilityAggregate Create(Guid eligibilityId, Guid ruleId, string ruleName, string description)
    {
        Guard.AgainstDefault(eligibilityId);
        Guard.AgainstDefault(ruleId);
        Guard.AgainstEmpty(ruleName);
        Guard.AgainstEmpty(description);

        var eligibility = new EligibilityAggregate();
        eligibility.Apply(new EligibilityGrantedEvent(eligibilityId, ruleName));
        eligibility.Rule = new EligibilityRule
        {
            Id = ruleId,
            RuleName = ruleName,
            Description = description
        };
        return eligibility;
    }

    public void Grant()
    {
        EnsureInvariant(
            Status != EligibilityStatus.Granted,
            "ALREADY_IN_STATE",
            "Eligibility is already granted.");

        Apply(new EligibilityGrantedEvent(Id, Rule?.RuleName ?? string.Empty));
    }

    public void Revoke(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(
            Status != EligibilityStatus.Revoked,
            "ALREADY_IN_STATE",
            "Eligibility is already revoked.");

        Apply(new EligibilityRevokedEvent(Id, reason));
    }

    private void Apply(EligibilityGrantedEvent e)
    {
        Id = e.EligibilityId;
        Status = EligibilityStatus.Granted;
        IsEligible = true;
        RaiseDomainEvent(e);
    }

    private void Apply(EligibilityRevokedEvent e)
    {
        Status = EligibilityStatus.Revoked;
        IsEligible = false;
        RaiseDomainEvent(e);
    }
}
