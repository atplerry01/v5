using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public sealed class SubclusterAggregate : AggregateRoot
{
    public SubclusterId Id { get; private set; }
    public SubclusterDescriptor Descriptor { get; private set; }
    public SubclusterStatus Status { get; private set; }
    public DateTimeOffset? AttachedAt { get; private set; }
    public AttachedUnder? AttachedUnder { get; private set; }

    public static SubclusterAggregate Define(SubclusterId id, SubclusterDescriptor descriptor)
    {
        var aggregate = new SubclusterAggregate();
        if (aggregate.Version >= 0)
            throw SubclusterErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new SubclusterDefinedEvent(id, descriptor));
        return aggregate;
    }

    public static SubclusterAggregate Define(SubclusterId id, SubclusterDescriptor descriptor, DateTimeOffset effectiveAt)
    {
        var aggregate = Define(id, descriptor);
        aggregate.RaiseDomainEvent(new SubclusterAttachedEvent(id, descriptor.ParentClusterReference, effectiveAt));
        return aggregate;
    }

    public static SubclusterAggregate Define(
        SubclusterId id,
        SubclusterDescriptor descriptor,
        DateTimeOffset effectiveAt,
        IStructuralParentLookup parentLookup)
    {
        Guard.Against(parentLookup is null, "IStructuralParentLookup must not be null.");

        var parent = descriptor.ParentClusterReference;

        var parentSpec = new CanAttachUnderParentSpecification(parentLookup!);
        if (!parentSpec.IsSatisfiedBy(parent))
            throw SubclusterErrors.InvalidParent();

        var aggregate = Define(id, descriptor, effectiveAt);

        aggregate.RaiseDomainEvent(new SubclusterBindingValidatedEvent(
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
            throw SubclusterErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new SubclusterActivatedEvent(Id));
    }

    public void Archive()
    {
        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SubclusterErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new SubclusterArchivedEvent(Id));
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SubclusterErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new SubclusterSuspendedEvent(Id));
    }

    public void Reactivate()
    {
        var specification = new CanReactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SubclusterErrors.InvalidStateTransition(Status, nameof(Reactivate));

        RaiseDomainEvent(new SubclusterReactivatedEvent(Id));
    }

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SubclusterErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new SubclusterRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SubclusterDefinedEvent e:
                Id = e.SubclusterId;
                Descriptor = e.Descriptor;
                Status = SubclusterStatus.Defined;
                break;
            case SubclusterActivatedEvent:
                Status = SubclusterStatus.Active;
                break;
            case SubclusterArchivedEvent:
                Status = SubclusterStatus.Archived;
                break;
            case SubclusterSuspendedEvent:
                Status = SubclusterStatus.Suspended;
                break;
            case SubclusterReactivatedEvent:
                Status = SubclusterStatus.Active;
                break;
            case SubclusterRetiredEvent:
                Status = SubclusterStatus.Retired;
                break;
            case SubclusterAttachedEvent e:
                AttachedAt = e.EffectiveAt;
                break;
            case SubclusterBindingValidatedEvent e:
                AttachedUnder = new AttachedUnder(e.Parent, e.ParentState, e.EffectiveAt);
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw SubclusterErrors.MissingId();

        if (Descriptor == default)
            throw SubclusterErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw SubclusterErrors.InvalidStateTransition(Status, "validate");
    }
}
