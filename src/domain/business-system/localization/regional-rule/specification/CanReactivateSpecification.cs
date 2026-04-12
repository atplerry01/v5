namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public sealed class CanReactivateSpecification
{
    public bool IsSatisfiedBy(RegionalRuleStatus status)
    {
        return status == RegionalRuleStatus.Suspended;
    }
}
