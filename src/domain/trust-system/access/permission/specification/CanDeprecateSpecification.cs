namespace Whycespace.Domain.TrustSystem.Access.Permission;

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(PermissionStatus status)
    {
        return status == PermissionStatus.Active;
    }
}
