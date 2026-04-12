namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(RuleStatus status)
    {
        return status == RuleStatus.Active;
    }
}
