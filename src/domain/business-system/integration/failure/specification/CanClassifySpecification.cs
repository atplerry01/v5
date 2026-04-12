namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public sealed class CanClassifySpecification
{
    public bool IsSatisfiedBy(FailureStatus status)
    {
        return status == FailureStatus.Detected;
    }
}
