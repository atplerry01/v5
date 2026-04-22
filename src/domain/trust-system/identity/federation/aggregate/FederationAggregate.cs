using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Federation;

public sealed class FederationAggregate : AggregateRoot
{
    public FederationId FederationId { get; private set; }
    public FederationDescriptor Descriptor { get; private set; }
    public FederationStatus Status { get; private set; }

    private FederationAggregate() { }

    public static FederationAggregate Establish(FederationId id, FederationDescriptor descriptor, Timestamp establishedAt)
    {
        var aggregate = new FederationAggregate();
        aggregate.RaiseDomainEvent(new FederationEstablishedEvent(id, descriptor, establishedAt));
        return aggregate;
    }

    public void Suspend()
    {
        if (Status != FederationStatus.Active)
            throw new DomainInvariantViolationException("Federation can only be suspended from Active status.");
        RaiseDomainEvent(new FederationSuspendedEvent(FederationId));
    }

    public void Terminate()
    {
        if (Status == FederationStatus.Terminated)
            throw new DomainInvariantViolationException("Federation is already terminated.");
        RaiseDomainEvent(new FederationTerminatedEvent(FederationId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case FederationEstablishedEvent e:
                FederationId = e.FederationId;
                Descriptor = e.Descriptor;
                Status = FederationStatus.Active;
                break;
            case FederationSuspendedEvent:
                Status = FederationStatus.Suspended;
                break;
            case FederationTerminatedEvent:
                Status = FederationStatus.Terminated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(FederationId == default, "Federation identity must be established.");
        Guard.Against(Descriptor == default, "Federation descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Federation status is not a defined enum value.");
    }
}
