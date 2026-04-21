using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public sealed class AdministrationAggregate : AggregateRoot
{
    public AdministrationId Id { get; private set; }
    public AdministrationDescriptor Descriptor { get; private set; }
    public AdministrationStatus Status { get; private set; }
    public DateTimeOffset? AttachedAt { get; private set; }
    public AttachedUnder? AttachedUnder { get; private set; }

    public static AdministrationAggregate Establish(AdministrationId id, AdministrationDescriptor descriptor)
    {
        var aggregate = new AdministrationAggregate();
        if (aggregate.Version >= 0)
            throw AdministrationErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AdministrationEstablishedEvent(id, descriptor));
        return aggregate;
    }

    public static AdministrationAggregate Establish(AdministrationId id, AdministrationDescriptor descriptor, DateTimeOffset effectiveAt)
    {
        var aggregate = Establish(id, descriptor);
        aggregate.RaiseDomainEvent(new AdministrationAttachedEvent(id, descriptor.ClusterReference, effectiveAt));
        return aggregate;
    }

    public static AdministrationAggregate Establish(
        AdministrationId id,
        AdministrationDescriptor descriptor,
        DateTimeOffset effectiveAt,
        IStructuralParentLookup parentLookup)
    {
        Guard.Against(parentLookup is null, "IStructuralParentLookup must not be null.");

        var parent = descriptor.ClusterReference;

        var parentSpec = new CanAttachUnderParentSpecification(parentLookup!);
        if (!parentSpec.IsSatisfiedBy(parent))
            throw AdministrationErrors.InvalidParent();

        var aggregate = Establish(id, descriptor, effectiveAt);

        aggregate.RaiseDomainEvent(new AdministrationBindingValidatedEvent(
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
            throw AdministrationErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new AdministrationActivatedEvent(Id));
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AdministrationErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new AdministrationSuspendedEvent(Id));
    }

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AdministrationErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new AdministrationRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AdministrationEstablishedEvent e:
                Id = e.AdministrationId;
                Descriptor = e.Descriptor;
                Status = AdministrationStatus.Established;
                break;
            case AdministrationActivatedEvent:
                Status = AdministrationStatus.Active;
                break;
            case AdministrationSuspendedEvent:
                Status = AdministrationStatus.Suspended;
                break;
            case AdministrationRetiredEvent:
                Status = AdministrationStatus.Retired;
                break;
            case AdministrationAttachedEvent e:
                AttachedAt = e.EffectiveAt;
                break;
            case AdministrationBindingValidatedEvent e:
                AttachedUnder = new AttachedUnder(e.Parent, e.ParentState, e.EffectiveAt);
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw AdministrationErrors.MissingId();

        if (Descriptor == default)
            throw AdministrationErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw AdministrationErrors.InvalidStateTransition(Status, "validate");
    }
}
