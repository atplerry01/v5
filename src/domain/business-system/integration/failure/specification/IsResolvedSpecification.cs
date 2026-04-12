namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public sealed class IsResolvedSpecification
{
    public bool IsSatisfiedBy(FailureStatus status)
    {
        return status == FailureStatus.Resolved;
    }
}
