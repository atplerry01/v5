namespace Whycespace.Domain.TrustSystem.Access.Role;

public sealed class RoleAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string ClusterId { get; private set; } = string.Empty;
    public RoleStatus Status { get; private set; } = RoleStatus.Active;
    public IReadOnlyList<Guid> PermissionIds => _permissionIds;

    private readonly List<Guid> _permissionIds = [];

    private RoleAggregate() { }

    public static RoleAggregate Create(
        Guid id,
        string name,
        string clusterId)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstEmpty(name);
        Guard.AgainstEmpty(clusterId);

        var aggregate = new RoleAggregate
        {
            Id = id,
            Name = name,
            ClusterId = clusterId,
            Status = RoleStatus.Active
        };

        aggregate.RaiseDomainEvent(new RoleCreatedEvent(id, name, clusterId));
        return aggregate;
    }

    public void AssignPermission(Guid permissionId)
    {
        Guard.AgainstDefault(permissionId);
        EnsureInvariant(Status == RoleStatus.Active, "ROLE_MUST_BE_ACTIVE", "Cannot modify inactive role.");

        if (_permissionIds.Contains(permissionId))
            return;

        _permissionIds.Add(permissionId);
        RaiseDomainEvent(new RolePermissionAssignedEvent(Id, permissionId));
    }

    public void RemovePermission(Guid permissionId)
    {
        EnsureInvariant(
            _permissionIds.Contains(permissionId),
            "PERM_NOT_ASSIGNED",
            $"Permission '{permissionId}' is not assigned to this role.");

        _permissionIds.Remove(permissionId);
        RaiseDomainEvent(new RolePermissionRemovedEvent(Id, permissionId));
    }

    public void Deactivate()
    {
        EnsureInvariant(Status == RoleStatus.Active, "ROLE_MUST_BE_ACTIVE", "Role is already inactive.");

        Status = RoleStatus.Inactive;
        RaiseDomainEvent(new RoleDeactivatedEvent(Id, Name));
    }
}
