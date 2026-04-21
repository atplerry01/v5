using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

public sealed class LifecycleAggregate : AggregateRoot
{
    public LifecycleId Id { get; private set; }
    public LifecycleDescriptor Descriptor { get; private set; }
    public LifecycleStatus Status { get; private set; }

    public static LifecycleAggregate Define(LifecycleId id, LifecycleDescriptor descriptor)
    {
        var aggregate = new LifecycleAggregate();
        if (aggregate.Version >= 0)
            throw LifecycleErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new LifecycleDefinedEvent(id, descriptor));
        return aggregate;
    }

    public void Transition()
    {
        var specification = new CanTransitionSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, nameof(Transition));

        RaiseDomainEvent(new LifecycleTransitionedEvent(Id));
    }

    public void Complete()
    {
        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, nameof(Complete));

        RaiseDomainEvent(new LifecycleCompletedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LifecycleDefinedEvent e:
                Id = e.LifecycleId;
                Descriptor = e.Descriptor;
                Status = LifecycleStatus.Defined;
                break;
            case LifecycleTransitionedEvent:
                Status = LifecycleStatus.Transitioned;
                break;
            case LifecycleCompletedEvent:
                Status = LifecycleStatus.Completed;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw LifecycleErrors.MissingId();

        if (Descriptor == default)
            throw LifecycleErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, "validate");
    }
}
