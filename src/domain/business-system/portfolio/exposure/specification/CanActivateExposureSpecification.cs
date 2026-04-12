namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public sealed class CanActivateExposureSpecification
{
    public bool IsSatisfiedBy(ExposureStatus status)
    {
        return status == ExposureStatus.Defined || status == ExposureStatus.Cleared;
    }
}
