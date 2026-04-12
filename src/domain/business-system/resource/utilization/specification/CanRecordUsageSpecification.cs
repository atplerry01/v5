namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed class CanRecordUsageSpecification
{
    public bool IsSatisfiedBy(UtilizationStatus status)
    {
        return status == UtilizationStatus.Recording;
    }
}
