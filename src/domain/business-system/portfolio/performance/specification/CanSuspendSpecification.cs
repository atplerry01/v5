namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(PerformanceStatus status)
    {
        return status == PerformanceStatus.Active;
    }
}
