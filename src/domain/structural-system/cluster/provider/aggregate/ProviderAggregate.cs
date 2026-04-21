using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public sealed class ProviderAggregate : AggregateRoot
{
    public ProviderId Id { get; private set; }
    public ProviderProfile Profile { get; private set; }
    public ProviderStatus Status { get; private set; }
    public DateTimeOffset? AttachedAt { get; private set; }
    public AttachedUnder? AttachedUnder { get; private set; }

    public static ProviderAggregate Register(ProviderId id, ProviderProfile profile)
    {
        var aggregate = new ProviderAggregate();
        if (aggregate.Version >= 0)
            throw ProviderErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ProviderRegisteredEvent(id, profile));
        return aggregate;
    }

    public static ProviderAggregate Register(ProviderId id, ProviderProfile profile, DateTimeOffset effectiveAt)
    {
        var aggregate = Register(id, profile);
        aggregate.RaiseDomainEvent(new ProviderAttachedEvent(id, profile.ClusterReference, effectiveAt));
        return aggregate;
    }

    public static ProviderAggregate Register(
        ProviderId id,
        ProviderProfile profile,
        DateTimeOffset effectiveAt,
        IStructuralParentLookup parentLookup)
    {
        Guard.Against(parentLookup is null, "IStructuralParentLookup must not be null.");

        var parent = profile.ClusterReference;

        var parentSpec = new CanAttachUnderParentSpecification(parentLookup!);
        if (!parentSpec.IsSatisfiedBy(parent))
            throw ProviderErrors.InvalidParent();

        var aggregate = Register(id, profile, effectiveAt);

        aggregate.RaiseDomainEvent(new ProviderBindingValidatedEvent(
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
            throw ProviderErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ProviderActivatedEvent(Id));
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new ProviderSuspendedEvent(Id));
    }

    public void Reactivate()
    {
        var specification = new CanReactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderErrors.InvalidStateTransition(Status, nameof(Reactivate));

        RaiseDomainEvent(new ProviderReactivatedEvent(Id));
    }

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new ProviderRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProviderRegisteredEvent e:
                Id = e.ProviderId;
                Profile = e.Profile;
                Status = ProviderStatus.Registered;
                break;
            case ProviderActivatedEvent:
                Status = ProviderStatus.Active;
                break;
            case ProviderSuspendedEvent:
                Status = ProviderStatus.Suspended;
                break;
            case ProviderReactivatedEvent:
                Status = ProviderStatus.Active;
                break;
            case ProviderRetiredEvent:
                Status = ProviderStatus.Retired;
                break;
            case ProviderAttachedEvent e:
                AttachedAt = e.EffectiveAt;
                break;
            case ProviderBindingValidatedEvent e:
                AttachedUnder = new AttachedUnder(e.Parent, e.ParentState, e.EffectiveAt);
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderErrors.MissingId();

        if (Profile == default)
            throw ProviderErrors.MissingProfile();

        if (!Enum.IsDefined(Status))
            throw ProviderErrors.InvalidStateTransition(Status, "validate");
    }
}
