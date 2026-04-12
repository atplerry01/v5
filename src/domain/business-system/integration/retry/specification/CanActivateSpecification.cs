namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(RetryStatus status)
    {
        return status is RetryStatus.Defined or RetryStatus.Disabled;
    }
}
