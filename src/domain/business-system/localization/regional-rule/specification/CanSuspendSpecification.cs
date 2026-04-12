namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(RegionalRuleStatus status)
    {
        return status == RegionalRuleStatus.Active;
    }
}
