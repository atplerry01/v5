namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class CanTerminateSpecification
{
    public bool IsSatisfiedBy(IdentityStatus status)
    {
        return status == IdentityStatus.Active || status == IdentityStatus.Suspended;
    }
}
