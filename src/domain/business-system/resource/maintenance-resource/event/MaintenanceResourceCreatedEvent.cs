namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public sealed record MaintenanceResourceCreatedEvent(
    MaintenanceResourceId MaintenanceResourceId,
    ResourceLink ResourceLink,
    MaintenanceRequirement Requirement);
