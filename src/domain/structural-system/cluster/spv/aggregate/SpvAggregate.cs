using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed class SpvAggregate : AggregateRoot
{
    public SpvId Id { get; private set; }
    public SpvDescriptor Descriptor { get; private set; }
    public SpvStatus Status { get; private set; }
    public DateTimeOffset? AttachedAt { get; private set; }
    public AttachedUnder? AttachedUnder { get; private set; }

    public static SpvAggregate Create(SpvId id, SpvDescriptor descriptor)
    {
        var aggregate = new SpvAggregate();
        if (aggregate.Version >= 0)
            throw SpvErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new SpvCreatedEvent(id, descriptor));
        return aggregate;
    }

    public static SpvAggregate Create(SpvId id, SpvDescriptor descriptor, DateTimeOffset effectiveAt)
    {
        var aggregate = Create(id, descriptor);
        aggregate.RaiseDomainEvent(new SpvAttachedEvent(id, descriptor.ClusterReference, effectiveAt));
        return aggregate;
    }

    public static SpvAggregate Create(
        SpvId id,
        SpvDescriptor descriptor,
        DateTimeOffset effectiveAt,
        IStructuralParentLookup parentLookup)
    {
        Guard.Against(parentLookup is null, "IStructuralParentLookup must not be null.");

        var parent = descriptor.ClusterReference;

        var parentSpec = new CanAttachUnderParentSpecification(parentLookup!);
        if (!parentSpec.IsSatisfiedBy(parent))
            throw SpvErrors.InvalidParent();

        var aggregate = Create(id, descriptor, effectiveAt);

        aggregate.RaiseDomainEvent(new SpvBindingValidatedEvent(
            id,
            parent,
            parentLookup!.GetState(parent),
            effectiveAt));

        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SpvErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new SpvActivatedEvent(Id));
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SpvErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new SpvSuspendedEvent(Id));
    }

    public void Close()
    {
        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SpvErrors.InvalidStateTransition(Status, nameof(Close));

        RaiseDomainEvent(new SpvClosedEvent(Id));
    }

    public void Reactivate()
    {
        var specification = new CanReactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SpvErrors.InvalidStateTransition(Status, nameof(Reactivate));

        RaiseDomainEvent(new SpvReactivatedEvent(Id));
    }

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SpvErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new SpvRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SpvCreatedEvent e:
                Id = e.SpvId;
                Descriptor = e.Descriptor;
                Status = SpvStatus.Created;
                break;
            case SpvActivatedEvent:
                Status = SpvStatus.Active;
                break;
            case SpvSuspendedEvent:
                Status = SpvStatus.Suspended;
                break;
            case SpvClosedEvent:
                Status = SpvStatus.Closed;
                break;
            case SpvReactivatedEvent:
                Status = SpvStatus.Active;
                break;
            case SpvRetiredEvent:
                Status = SpvStatus.Retired;
                break;
            case SpvAttachedEvent e:
                AttachedAt = e.EffectiveAt;
                break;
            case SpvBindingValidatedEvent e:
                AttachedUnder = new AttachedUnder(e.Parent, e.ParentState, e.EffectiveAt);
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw SpvErrors.MissingId();

        if (Descriptor == default)
            throw SpvErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw SpvErrors.InvalidStateTransition(Status, "validate");
    }
}
