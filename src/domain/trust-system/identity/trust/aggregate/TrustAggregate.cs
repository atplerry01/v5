using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed class TrustAggregate : AggregateRoot
{
    public TrustId TrustId { get; private set; }
    public TrustDescriptor Descriptor { get; private set; }
    public TrustStatus Status { get; private set; }

    private TrustAggregate() { }

    public static TrustAggregate Assess(TrustId id, TrustDescriptor descriptor, Timestamp assessedAt)
    {
        var aggregate = new TrustAggregate();
        aggregate.RaiseDomainEvent(new TrustAssessedEvent(id, descriptor, assessedAt));
        return aggregate;
    }

    public void Activate()
    {
        if (Status != TrustStatus.Assessed)
            throw new DomainInvariantViolationException("Trust can only be activated from Assessed status.");
        RaiseDomainEvent(new TrustActivatedEvent(TrustId));
    }

    public void Suspend()
    {
        if (Status != TrustStatus.Active)
            throw new DomainInvariantViolationException("Trust can only be suspended from Active status.");
        RaiseDomainEvent(new TrustSuspendedEvent(TrustId));
    }

    public void Revoke()
    {
        if (Status == TrustStatus.Revoked || Status == TrustStatus.Assessed)
            throw new DomainInvariantViolationException("Trust cannot be revoked from its current status.");
        RaiseDomainEvent(new TrustRevokedEvent(TrustId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TrustAssessedEvent e:
                TrustId = e.TrustId;
                Descriptor = e.Descriptor;
                Status = TrustStatus.Assessed;
                break;
            case TrustActivatedEvent:
                Status = TrustStatus.Active;
                break;
            case TrustSuspendedEvent:
                Status = TrustStatus.Suspended;
                break;
            case TrustRevokedEvent:
                Status = TrustStatus.Revoked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(TrustId == default, "Trust identity must be established.");
        Guard.Against(Descriptor == default, "Trust descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Trust status is not a defined enum value.");
    }
}
