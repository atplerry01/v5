namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public static class ServiceIdentityErrors
{
    public static DomainException NotFound(Guid id)
        => new("SERVICE_IDENTITY_NOT_FOUND", $"Service identity '{id}' was not found.");

    public static DomainException Suspended(Guid id)
        => new("SERVICE_IDENTITY_SUSPENDED", $"Service identity '{id}' is suspended.");

    public static DomainException Decommissioned(Guid id)
        => new("SERVICE_IDENTITY_DECOMMISSIONED", $"Service identity '{id}' is decommissioned.");
}
