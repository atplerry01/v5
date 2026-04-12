namespace Whycespace.Domain.BusinessSystem.Integration.Synchronization;

public sealed class CanConfirmSpecification
{
    public bool IsSatisfiedBy(SynchronizationStatus status)
    {
        return status == SynchronizationStatus.Pending;
    }
}
