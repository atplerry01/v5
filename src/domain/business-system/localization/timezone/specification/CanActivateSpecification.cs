namespace Whycespace.Domain.BusinessSystem.Localization.Timezone;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(TimezoneStatus status)
    {
        return status == TimezoneStatus.Draft;
    }
}
