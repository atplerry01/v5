using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.AccessControl.Authorization;

public sealed class AuthorizationAggregate : AggregateRoot
{
    public AuthorizationId Id { get; private set; }
    public string SubjectId { get; private set; } = string.Empty;
    public IReadOnlySet<string> RoleIds { get; private set; } = new HashSet<string>();
    public DateTimeOffset ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public bool IsRevoked { get; private set; }

    private AuthorizationAggregate() { }

    public static AuthorizationAggregate Grant(
        AuthorizationId id,
        string subjectId,
        IEnumerable<string> roleIds,
        DateTimeOffset validFrom,
        DateTimeOffset? validTo = null)
    {
        var roleSet = roleIds.ToHashSet();
        Guard.Against(string.IsNullOrEmpty(subjectId), "Authorization requires a subjectId.");
        Guard.Against(roleSet.Count == 0, "Authorization requires at least one role.");
        Guard.Against(validTo.HasValue && validTo.Value <= validFrom, "Authorization validTo must be after validFrom.");

        var aggregate = new AuthorizationAggregate();
        aggregate.RaiseDomainEvent(new AuthorizationGrantedEvent(id, subjectId, roleSet, validFrom, validTo));
        return aggregate;
    }

    public void Revoke()
    {
        Guard.Against(IsRevoked, "Authorization is already revoked.");
        RaiseDomainEvent(new AuthorizationRevokedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AuthorizationGrantedEvent e:
                Id = e.Id; SubjectId = e.SubjectId; RoleIds = e.RoleIds; ValidFrom = e.ValidFrom; ValidTo = e.ValidTo;
                break;
            case AuthorizationRevokedEvent:
                IsRevoked = true;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "Authorization must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(SubjectId), "Authorization must have a subjectId.");
    }
}
