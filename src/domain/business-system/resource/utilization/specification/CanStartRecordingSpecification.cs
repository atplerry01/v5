namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed class CanStartRecordingSpecification
{
    public bool IsSatisfiedBy(UtilizationStatus status)
    {
        return status == UtilizationStatus.Initiated;
    }
}
