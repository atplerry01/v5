using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public sealed class StateSnapshotAggregate : AggregateRoot
{
    public StateSnapshotId SnapshotId { get; private set; }
    public SnapshotDescriptor Descriptor { get; private set; }
    public SnapshotStatus Status { get; private set; }

    public static StateSnapshotAggregate Capture(StateSnapshotId id, SnapshotDescriptor descriptor)
    {
        var aggregate = new StateSnapshotAggregate();
        if (aggregate.Version >= 0)
            throw StateSnapshotErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new StateSnapshotCapturedEvent(id, descriptor));
        return aggregate;
    }

    public void Verify()
    {
        var specification = new CanVerifySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StateSnapshotErrors.InvalidStateTransition(Status, "Verify");

        RaiseDomainEvent(new StateSnapshotVerifiedEvent(SnapshotId));
    }

    public void Expire()
    {
        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StateSnapshotErrors.InvalidStateTransition(Status, "Expire");

        RaiseDomainEvent(new StateSnapshotExpiredEvent(SnapshotId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case StateSnapshotCapturedEvent e:
                SnapshotId = e.SnapshotId;
                Descriptor = e.Descriptor;
                Status = SnapshotStatus.Captured;
                break;
            case StateSnapshotVerifiedEvent:
                Status = SnapshotStatus.Verified;
                break;
            case StateSnapshotExpiredEvent:
                Status = SnapshotStatus.Expired;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (SnapshotId == default)
            throw StateSnapshotErrors.MissingId();

        if (Descriptor == default)
            throw StateSnapshotErrors.MissingDescriptor();
    }
}
