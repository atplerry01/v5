namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(RuleStatus status)
    {
        return status == RuleStatus.Draft;
    }
}
