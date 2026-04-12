namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public sealed class CanEnforceSpecification
{
    public bool IsSatisfiedBy(PreferenceStatus status)
    {
        return status == PreferenceStatus.Draft;
    }
}
