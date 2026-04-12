namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public sealed class PerformanceSpecification
{
    public bool IsSatisfiedBy(PerformanceId id, PerformanceName name)
    {
        return id.Value != Guid.Empty
            && !string.IsNullOrWhiteSpace(name.Value);
    }
}
