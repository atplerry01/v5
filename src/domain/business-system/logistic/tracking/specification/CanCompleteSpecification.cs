namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(TrackingStatus status)
    {
        return status == TrackingStatus.Active || status == TrackingStatus.Paused;
    }
}
