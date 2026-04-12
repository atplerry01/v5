namespace Whycespace.Domain.TrustSystem.Access.Authorization;

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(AuthorizationStatus status)
    {
        return status == AuthorizationStatus.Granted;
    }
}
