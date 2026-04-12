namespace Whycespace.Domain.BusinessSystem.Localization.Timezone;

public sealed class CanDeactivateSpecification
{
    public bool IsSatisfiedBy(TimezoneStatus status)
    {
        return status == TimezoneStatus.Active;
    }
}
