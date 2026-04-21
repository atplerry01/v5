using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed class AuthorityAggregate : AggregateRoot
{
    public AuthorityId Id { get; private set; }
    public AuthorityDescriptor Descriptor { get; private set; }
    public AuthorityStatus Status { get; private set; }
    public DateTimeOffset? AttachedAt { get; private set; }
    public AttachedUnder? AttachedUnder { get; private set; }

    public static AuthorityAggregate Establish(AuthorityId id, AuthorityDescriptor descriptor)
    {
        var aggregate = new AuthorityAggregate();
        if (aggregate.Version >= 0)
            throw AuthorityErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AuthorityEstablishedEvent(id, descriptor));
        return aggregate;
    }

    public static AuthorityAggregate Establish(AuthorityId id, AuthorityDescriptor descriptor, DateTimeOffset effectiveAt)
    {
        var aggregate = Establish(id, descriptor);
        aggregate.RaiseDomainEvent(new AuthorityAttachedEvent(id, descriptor.ClusterReference, effectiveAt));
        return aggregate;
    }

    public static AuthorityAggregate Establish(
        AuthorityId id,
        AuthorityDescriptor descriptor,
        DateTimeOffset effectiveAt,
        IStructuralParentLookup parentLookup)
    {
        Guard.Against(parentLookup is null, "IStructuralParentLookup must not be null.");

        var parent = descriptor.ClusterReference;

        var parentSpec = new CanAttachUnderParentSpecification(parentLookup!);
        if (!parentSpec.IsSatisfiedBy(parent))
            throw AuthorityErrors.InvalidParent();

        var aggregate = Establish(id, descriptor, effectiveAt);

        aggregate.RaiseDomainEvent(new AuthorityBindingValidatedEvent(
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
            throw AuthorityErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new AuthorityActivatedEvent(Id));
    }

    public void Revoke()
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AuthorityErrors.InvalidStateTransition(Status, nameof(Revoke));

        RaiseDomainEvent(new AuthorityRevokedEvent(Id));
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AuthorityErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new AuthoritySuspendedEvent(Id));
    }

    public void Reactivate()
    {
        var specification = new CanReactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AuthorityErrors.InvalidStateTransition(Status, nameof(Reactivate));

        RaiseDomainEvent(new AuthorityReactivatedEvent(Id));
    }

    public void Retire()
    {
        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AuthorityErrors.InvalidStateTransition(Status, nameof(Retire));

        RaiseDomainEvent(new AuthorityRetiredEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AuthorityEstablishedEvent e:
                Id = e.AuthorityId;
                Descriptor = e.Descriptor;
                Status = AuthorityStatus.Established;
                break;
            case AuthorityActivatedEvent:
                Status = AuthorityStatus.Active;
                break;
            case AuthorityRevokedEvent:
                Status = AuthorityStatus.Revoked;
                break;
            case AuthoritySuspendedEvent:
                Status = AuthorityStatus.Suspended;
                break;
            case AuthorityReactivatedEvent:
                Status = AuthorityStatus.Active;
                break;
            case AuthorityRetiredEvent:
                Status = AuthorityStatus.Retired;
                break;
            case AuthorityAttachedEvent e:
                AttachedAt = e.EffectiveAt;
                break;
            case AuthorityBindingValidatedEvent e:
                AttachedUnder = new AttachedUnder(e.Parent, e.ParentState, e.EffectiveAt);
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw AuthorityErrors.MissingId();

        if (Descriptor == default)
            throw AuthorityErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw AuthorityErrors.InvalidStateTransition(Status, "validate");
    }
}
