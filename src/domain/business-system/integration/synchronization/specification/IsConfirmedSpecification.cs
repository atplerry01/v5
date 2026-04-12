namespace Whycespace.Domain.BusinessSystem.Integration.Synchronization;

public sealed class IsConfirmedSpecification
{
    public bool IsSatisfiedBy(SynchronizationStatus status)
    {
        return status == SynchronizationStatus.Confirmed;
    }
}
