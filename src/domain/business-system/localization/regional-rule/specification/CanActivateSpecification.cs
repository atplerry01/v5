namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(RegionalRuleStatus status)
    {
        return status == RegionalRuleStatus.Draft;
    }
}
