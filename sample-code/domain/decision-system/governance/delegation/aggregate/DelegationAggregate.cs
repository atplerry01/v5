using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;
using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.DecisionSystem.Governance.Delegation;

public class DelegationAggregate : AggregateRoot
{
    public IdentityId DelegatorIdentityId { get; private set; } = default!;
    public IdentityId DelegateeIdentityId { get; private set; } = default!;
    public JurisdictionId? JurisdictionId { get; private set; }
    public bool IsActive { get; private set; }

    protected DelegationAggregate() { }

    public static DelegationAggregate Create(
        Guid delegationId,
        IdentityId delegatorIdentityId,
        IdentityId delegateeIdentityId,
        JurisdictionId? jurisdictionId = null)
    {
        Guard.AgainstDefault(delegationId);
        Guard.AgainstNull(delegatorIdentityId);
        Guard.AgainstNull(delegateeIdentityId);
        Guard.AgainstInvalid(
            delegateeIdentityId,
            d => d.Value != delegatorIdentityId.Value,
            "Cannot delegate to self.");

        var delegation = new DelegationAggregate();
        delegation.Apply(new DelegationCreatedEvent(delegationId, delegatorIdentityId.Value, delegateeIdentityId.Value));
        delegation.JurisdictionId = jurisdictionId;
        return delegation;
    }

    public void Revoke(IdentityId revokerIdentityId)
    {
        Guard.AgainstNull(revokerIdentityId);
        EnsureInvariant(IsActive, "ALREADY_REVOKED", "Delegation is already revoked.");

        EnsureInvariant(
            revokerIdentityId.Value == DelegatorIdentityId.Value,
            "UNAUTHORIZED",
            "Only the delegator can revoke a delegation.");

        Apply(new DelegationRevokedEvent(Id, revokerIdentityId.Value));
    }

    private void Apply(DelegationCreatedEvent e)
    {
        Id = e.DelegationId;
        DelegatorIdentityId = new IdentityId(e.DelegatorIdentityId);
        DelegateeIdentityId = new IdentityId(e.DelegateeIdentityId);
        IsActive = true;
        RaiseDomainEvent(e);
    }

    private void Apply(DelegationRevokedEvent e)
    {
        IsActive = false;
        RaiseDomainEvent(e);
    }
}
