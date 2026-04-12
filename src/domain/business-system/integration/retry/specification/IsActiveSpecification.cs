namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(RetryStatus status)
    {
        return status == RetryStatus.Active;
    }
}
