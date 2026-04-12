namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public sealed class CanResumeSpecification
{
    public bool IsSatisfiedBy(MaintenanceResourceStatus status)
    {
        return status == MaintenanceResourceStatus.Suspended;
    }
}
