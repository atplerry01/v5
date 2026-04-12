namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public sealed class CanResumeSpecification
{
    public bool IsSatisfiedBy(TrackingStatus status)
    {
        return status == TrackingStatus.Paused;
    }
}
