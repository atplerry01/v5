namespace Whycespace.Domain.TrustSystem.Access.Grant;

public sealed class AccessAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public string Resource { get; private set; } = string.Empty;
    public string Permission { get; private set; } = string.Empty;
    public bool IsRevoked { get; private set; }

    private AccessAggregate() { }

    public static AccessAggregate Grant(Guid id, Guid identityId, string resource, string permission)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstDefault(identityId);
        Guard.AgainstEmpty(resource);
        Guard.AgainstEmpty(permission);

        var aggregate = new AccessAggregate
        {
            Id = id,
            IdentityId = identityId,
            Resource = resource,
            Permission = permission,
            IsRevoked = false
        };

        aggregate.RaiseDomainEvent(new AccessGrantedEvent(id, identityId, resource, permission));
        return aggregate;
    }

    public void Revoke(string reason)
    {
        EnsureInvariant(!IsRevoked, "GRANT_MUST_BE_ACTIVE", "Access grant is already revoked.");

        IsRevoked = true;
        RaiseDomainEvent(new AccessRevokedEvent(Id, IdentityId, Resource, Permission, reason));
    }
}
