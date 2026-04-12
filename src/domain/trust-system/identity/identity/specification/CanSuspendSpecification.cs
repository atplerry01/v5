namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(IdentityStatus status)
    {
        return status == IdentityStatus.Active;
    }
}
