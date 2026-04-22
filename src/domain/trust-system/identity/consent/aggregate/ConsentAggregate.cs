using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed class ConsentAggregate : AggregateRoot
{
    public ConsentId ConsentId { get; private set; }
    public ConsentDescriptor Descriptor { get; private set; }
    public ConsentStatus Status { get; private set; }

    private ConsentAggregate() { }

    public static ConsentAggregate Grant(ConsentId id, ConsentDescriptor descriptor, Timestamp grantedAt)
    {
        var aggregate = new ConsentAggregate();
        aggregate.RaiseDomainEvent(new ConsentGrantedEvent(id, descriptor, grantedAt));
        return aggregate;
    }

    public void Revoke()
    {
        if (Status != ConsentStatus.Granted)
            throw new DomainInvariantViolationException("Consent can only be revoked from Granted status.");
        RaiseDomainEvent(new ConsentRevokedEvent(ConsentId));
    }

    public void Expire()
    {
        if (Status != ConsentStatus.Granted)
            throw new DomainInvariantViolationException("Consent can only be expired from Granted status.");
        RaiseDomainEvent(new ConsentExpiredEvent(ConsentId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ConsentGrantedEvent e:
                ConsentId = e.ConsentId;
                Descriptor = e.Descriptor;
                Status = ConsentStatus.Granted;
                break;
            case ConsentRevokedEvent:
                Status = ConsentStatus.Revoked;
                break;
            case ConsentExpiredEvent:
                Status = ConsentStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(ConsentId == default, "Consent identity must be established.");
        Guard.Against(Descriptor == default, "Consent descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Consent status is not a defined enum value.");
    }
}
