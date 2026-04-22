using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Role;

public sealed class RoleAggregate : AggregateRoot
{
    public RoleId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IReadOnlySet<string> PermissionIds { get; private set; } = new HashSet<string>();
    public string? ParentRoleId { get; private set; }
    public bool IsDeprecated { get; private set; }

    private RoleAggregate() { }

    public static RoleAggregate Define(RoleId id, string name, IEnumerable<string> permissionIds, string? parentRoleId = null)
    {
        Guard.Against(string.IsNullOrEmpty(name), "Role name must not be empty.");

        var aggregate = new RoleAggregate();
        aggregate.RaiseDomainEvent(new RoleDefinedEvent(id, name, permissionIds.ToHashSet(), parentRoleId));
        return aggregate;
    }

    public void AddPermission(string permissionId)
    {
        Guard.Against(string.IsNullOrEmpty(permissionId), "PermissionId must not be empty.");
        Guard.Against(IsDeprecated, "Cannot add permissions to a deprecated role.");

        if (!PermissionIds.Contains(permissionId))
            RaiseDomainEvent(new RolePermissionAddedEvent(Id, permissionId));
    }

    public void Deprecate()
    {
        Guard.Against(IsDeprecated, "Role is already deprecated.");
        RaiseDomainEvent(new RoleDeprecatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RoleDefinedEvent e:
                Id = e.Id; Name = e.Name; PermissionIds = e.PermissionIds; ParentRoleId = e.ParentRoleId;
                break;
            case RolePermissionAddedEvent e:
                PermissionIds = new HashSet<string>(PermissionIds) { e.PermissionId };
                break;
            case RoleDeprecatedEvent:
                IsDeprecated = true;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "Role must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(Name), "Role must have a non-empty Name.");
        Guard.Against(ParentRoleId == Id.Value, "A role cannot be its own parent.");
    }
}
