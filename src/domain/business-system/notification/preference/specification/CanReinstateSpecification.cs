namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public sealed class CanReinstateSpecification
{
    public bool IsSatisfiedBy(PreferenceStatus status)
    {
        return status == PreferenceStatus.Suspended;
    }
}
