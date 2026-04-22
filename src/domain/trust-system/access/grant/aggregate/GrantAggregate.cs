using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Grant;

public sealed class GrantAggregate : AggregateRoot
{
    public GrantId GrantId { get; private set; }
    public GrantDescriptor Descriptor { get; private set; }
    public GrantStatus Status { get; private set; }

    private GrantAggregate() { }

    public static GrantAggregate Issue(GrantId id, GrantDescriptor descriptor, Timestamp issuedAt)
    {
        var aggregate = new GrantAggregate();
        aggregate.RaiseDomainEvent(new GrantIssuedEvent(id, descriptor, issuedAt));
        return aggregate;
    }

    public void Activate()
    {
        if (Status != GrantStatus.Issued)
            throw new DomainInvariantViolationException("Grant can only be activated from Issued status.");
        RaiseDomainEvent(new GrantActivatedEvent(GrantId));
    }

    public void Revoke()
    {
        if (Status == GrantStatus.Revoked || Status == GrantStatus.Expired)
            throw new DomainInvariantViolationException("Grant cannot be revoked from its current status.");
        RaiseDomainEvent(new GrantRevokedEvent(GrantId));
    }

    public void Expire()
    {
        if (Status == GrantStatus.Revoked || Status == GrantStatus.Expired)
            throw new DomainInvariantViolationException("Grant cannot be expired from its current status.");
        RaiseDomainEvent(new GrantExpiredEvent(GrantId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case GrantIssuedEvent e:
                GrantId = e.GrantId;
                Descriptor = e.Descriptor;
                Status = GrantStatus.Issued;
                break;
            case GrantActivatedEvent:
                Status = GrantStatus.Active;
                break;
            case GrantRevokedEvent:
                Status = GrantStatus.Revoked;
                break;
            case GrantExpiredEvent:
                Status = GrantStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(GrantId == default, "Grant identity must be established.");
        Guard.Against(Descriptor == default, "Grant descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Grant status is not a defined enum value.");
    }
}
