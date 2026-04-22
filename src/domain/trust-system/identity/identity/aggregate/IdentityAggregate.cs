using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class IdentityAggregate : AggregateRoot
{
    public IdentityId Id { get; private set; }
    public IdentityDescriptor Descriptor { get; private set; }
    public IdentityStatus Status { get; private set; }

    private IdentityAggregate() { }

    public static IdentityAggregate Establish(IdentityId id, IdentityDescriptor descriptor)
    {
        var aggregate = new IdentityAggregate();
        aggregate.RaiseDomainEvent(new IdentityEstablishedEvent(id, descriptor));
        return aggregate;
    }

    public void Suspend()
    {
        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw IdentityErrors.InvalidStateTransition(Status, nameof(Suspend));

        RaiseDomainEvent(new IdentitySuspendedEvent(Id));
    }

    public void Terminate()
    {
        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw IdentityErrors.InvalidStateTransition(Status, nameof(Terminate));

        RaiseDomainEvent(new IdentityTerminatedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case IdentityEstablishedEvent e:
                Id = e.IdentityId;
                Descriptor = e.Descriptor;
                Status = IdentityStatus.Active;
                break;
            case IdentitySuspendedEvent:
                Status = IdentityStatus.Suspended;
                break;
            case IdentityTerminatedEvent:
                Status = IdentityStatus.Terminated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw IdentityErrors.MissingId();

        if (Descriptor == default)
            throw IdentityErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw IdentityErrors.InvalidStateTransition(Status, "validate");
    }

    protected override void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
        // Currently no additional pre-conditions beyond specification checks.
    }
}
