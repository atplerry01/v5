namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(MaintenanceResourceStatus status)
    {
        return status == MaintenanceResourceStatus.Active;
    }
}
