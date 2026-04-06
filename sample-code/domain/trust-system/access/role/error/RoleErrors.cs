namespace Whycespace.Domain.TrustSystem.Access.Role;

public static class RoleErrors
{
    public static DomainException AlreadyExists(string name, string clusterId)
        => new("ROLE.ALREADY_EXISTS", $"Role '{name}' already exists in cluster '{clusterId}'.");

    public static DomainException NotFound(Guid roleId)
        => new("ROLE.NOT_FOUND", $"Role '{roleId}' not found.");

    public static DomainException CannotDeleteWithAssignments(Guid roleId)
        => new("ROLE.HAS_ASSIGNMENTS", $"Role '{roleId}' cannot be deleted while it has active assignments.");
}
