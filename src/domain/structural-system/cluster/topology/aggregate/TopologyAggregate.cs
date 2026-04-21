using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed class TopologyAggregate : AggregateRoot
{
    public TopologyId Id { get; private set; }
    public TopologyDescriptor Descriptor { get; private set; }
    public TopologyStatus Status { get; private set; }

    public static TopologyAggregate Define(TopologyId id, TopologyDescriptor descriptor)
    {
        var aggregate = new TopologyAggregate();
        if (aggregate.Version >= 0)
            throw TopologyErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new TopologyDefinedEvent(id, descriptor));
        return aggregate;
    }

    public void Validate()
    {
        var specification = new CanValidateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TopologyErrors.InvalidStateTransition(Status, nameof(Validate));

        RaiseDomainEvent(new TopologyValidatedEvent(Id));
    }

    public void Lock()
    {
        var specification = new CanLockSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TopologyErrors.InvalidStateTransition(Status, nameof(Lock));

        RaiseDomainEvent(new TopologyLockedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case TopologyDefinedEvent e:
                Id = e.TopologyId;
                Descriptor = e.Descriptor;
                Status = TopologyStatus.Defined;
                break;
            case TopologyValidatedEvent:
                Status = TopologyStatus.Validated;
                break;
            case TopologyLockedEvent:
                Status = TopologyStatus.Locked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw TopologyErrors.MissingId();

        if (Descriptor == default)
            throw TopologyErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw TopologyErrors.InvalidStateTransition(Status, "validate");
    }
}
