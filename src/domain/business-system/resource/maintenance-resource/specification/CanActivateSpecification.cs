namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(MaintenanceResourceStatus status)
    {
        return status == MaintenanceResourceStatus.Defined;
    }
}
