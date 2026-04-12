namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(PreferenceStatus status)
    {
        return status == PreferenceStatus.Enforced;
    }
}
