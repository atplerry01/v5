namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public sealed class CanDisableSpecification
{
    public bool IsSatisfiedBy(RetryStatus status)
    {
        return status == RetryStatus.Active;
    }
}
