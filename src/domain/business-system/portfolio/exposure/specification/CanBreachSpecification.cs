namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public sealed class CanBreachSpecification
{
    public bool IsSatisfiedBy(ExposureStatus status)
    {
        return status == ExposureStatus.Active;
    }
}
