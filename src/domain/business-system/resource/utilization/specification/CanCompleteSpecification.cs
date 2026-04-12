namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(UtilizationStatus status)
    {
        return status == UtilizationStatus.Recording;
    }
}
