namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(PerformanceStatus status)
    {
        return status == PerformanceStatus.Draft;
    }
}
