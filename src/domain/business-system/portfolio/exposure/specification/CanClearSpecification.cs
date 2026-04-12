namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public sealed class CanClearSpecification
{
    public bool IsSatisfiedBy(ExposureStatus status)
    {
        return status == ExposureStatus.Breached;
    }
}
