using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

public sealed class IdentityGraphAggregate : AggregateRoot
{
    public IdentityGraphId IdentityGraphId { get; private set; }
    public IdentityGraphDescriptor Descriptor { get; private set; }
    public IdentityGraphStatus Status { get; private set; }

    private IdentityGraphAggregate() { }

    public static IdentityGraphAggregate Initialize(IdentityGraphId id, IdentityGraphDescriptor descriptor, Timestamp initializedAt)
    {
        var aggregate = new IdentityGraphAggregate();
        aggregate.RaiseDomainEvent(new IdentityGraphInitializedEvent(id, descriptor, initializedAt));
        return aggregate;
    }

    public void Archive()
    {
        if (Status != IdentityGraphStatus.Active)
            throw new DomainInvariantViolationException("Identity graph can only be archived from Active status.");
        RaiseDomainEvent(new IdentityGraphArchivedEvent(IdentityGraphId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case IdentityGraphInitializedEvent e:
                IdentityGraphId = e.IdentityGraphId;
                Descriptor = e.Descriptor;
                Status = IdentityGraphStatus.Active;
                break;
            case IdentityGraphArchivedEvent:
                Status = IdentityGraphStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(IdentityGraphId == default, "Identity graph identity must be established.");
        Guard.Against(Descriptor == default, "Identity graph descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Identity graph status is not a defined enum value.");
    }
}
