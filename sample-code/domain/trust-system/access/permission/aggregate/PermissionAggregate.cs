namespace Whycespace.Domain.TrustSystem.Access.Permission;

public sealed class PermissionAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public PermissionScope Scope { get; private set; } = PermissionScope.Cluster;
    public bool IsActive { get; private set; }

    private PermissionAggregate() { }

    public static PermissionAggregate Define(
        Guid id,
        string name,
        string resource,
        string action,
        PermissionScope scope)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstEmpty(name);
        Guard.AgainstEmpty(resource);
        Guard.AgainstEmpty(action);

        var aggregate = new PermissionAggregate
        {
            Id = id,
            Name = name,
            Resource = resource,
            Action = action,
            Scope = scope,
            IsActive = true
        };

        aggregate.RaiseDomainEvent(new PermissionDefinedEvent(id, name, resource, action, scope.Value));
        return aggregate;
    }

    public void Revoke(DateTimeOffset timestamp)
    {
        EnsureInvariant(IsActive, "PERM_MUST_BE_ACTIVE", "Only active permissions can be revoked.");

        IsActive = false;
        RaiseDomainEvent(new PermissionRevokedEvent(Id, Name));
    }
}
