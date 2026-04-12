namespace Whycespace.Domain.BusinessSystem.Resource.MaintenanceResource;

public static class MaintenanceResourceErrors
{
    public static MaintenanceResourceDomainException MissingId()
        => new("MaintenanceResourceId is required and must not be empty.");

    public static MaintenanceResourceDomainException InvalidStateTransition(MaintenanceResourceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static MaintenanceResourceDomainException RequirementRequired()
        => new("Maintenance resource must define a maintenance requirement.");

    public static MaintenanceResourceDomainException ResourceLinkRequired()
        => new("Maintenance resource must link to a resource or equipment.");
}

public sealed class MaintenanceResourceDomainException : Exception
{
    public MaintenanceResourceDomainException(string message) : base(message) { }
}
