namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class IdentityService
{
    public bool CanActivate(IdentityAggregate identity)
        => identity.Status == IdentityStatus.Pending;

    public bool CanSuspend(IdentityAggregate identity)
        => identity.Status == IdentityStatus.Active;

    public bool CanDeactivate(IdentityAggregate identity)
        => identity.Status != IdentityStatus.Deactivated;
}
