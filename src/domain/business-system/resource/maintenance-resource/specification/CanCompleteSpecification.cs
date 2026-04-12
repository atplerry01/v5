namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(MaintenanceResourceStatus status)
    {
        return status == MaintenanceResourceStatus.Active;
    }
}
