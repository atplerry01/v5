namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public sealed class CanResumeSpecification
{
    public bool IsSatisfiedBy(PerformanceStatus status)
    {
        return status == PerformanceStatus.Suspended;
    }
}
