namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public sealed class CanCloseSpecification
{
    public bool IsSatisfiedBy(PerformanceStatus status)
    {
        return status == PerformanceStatus.Active;
    }
}
