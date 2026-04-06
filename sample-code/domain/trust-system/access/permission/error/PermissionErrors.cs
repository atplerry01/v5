namespace Whycespace.Domain.TrustSystem.Access.Permission;

public static class PermissionErrors
{
    public static DomainException AlreadyExists(string name)
        => new("PERM.ALREADY_EXISTS", $"Permission '{name}' already exists.");

    public static DomainException NotFound(Guid permissionId)
        => new("PERM.NOT_FOUND", $"Permission '{permissionId}' not found.");

    public static DomainException InsufficientScope(string required, string actual)
        => new("PERM.INSUFFICIENT_SCOPE", $"Required scope '{required}' exceeds granted scope '{actual}'.");
}
