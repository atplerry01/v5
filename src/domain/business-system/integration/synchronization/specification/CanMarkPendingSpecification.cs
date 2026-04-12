namespace Whycespace.Domain.BusinessSystem.Integration.Synchronization;

public sealed class CanMarkPendingSpecification
{
    public bool IsSatisfiedBy(SynchronizationStatus status)
    {
        return status == SynchronizationStatus.Defined;
    }
}
