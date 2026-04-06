namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed class ServiceIdentityService
{
    public bool CanAuthenticate(ServiceIdentityAggregate identity, DateTimeOffset now)
        => identity.Status == ServiceIdentityStatus.Active
           && identity.Credentials.Any(c => c.IsActive && c.ExpiresAt > now);
}
