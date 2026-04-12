namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(RegionalRuleStatus status)
    {
        return status is RegionalRuleStatus.Active or RegionalRuleStatus.Suspended;
    }
}
