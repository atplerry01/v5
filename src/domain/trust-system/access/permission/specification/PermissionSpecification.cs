namespace Whycespace.Domain.TrustSystem.Access.Permission;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(PermissionStatus status)
    {
        return status == PermissionStatus.Defined;
    }
}
