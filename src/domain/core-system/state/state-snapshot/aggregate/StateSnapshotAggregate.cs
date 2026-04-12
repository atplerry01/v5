namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public sealed class StateSnapshotAggregate
{
    public StateSnapshotId SnapshotId { get; private set; }
    public SnapshotDescriptor Descriptor { get; private set; }
    public SnapshotStatus Status { get; private set; }

    private readonly List<object> _domainEvents = new();
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    private StateSnapshotAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static StateSnapshotAggregate Capture(StateSnapshotId id, SnapshotDescriptor descriptor)
    {
        var aggregate = new StateSnapshotAggregate();
        var @event = new StateSnapshotCapturedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.EnsureInvariants();
        aggregate._domainEvents.Add(@event);
        return aggregate;
    }

    // ── Transitions ─────────────────────────────────────────────

    public void Verify()
    {
        var specification = new CanVerifySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StateSnapshotErrors.InvalidStateTransition(Status, "Verify");

        var @event = new StateSnapshotVerifiedEvent(SnapshotId);
        Apply(@event);
        EnsureInvariants();
        _domainEvents.Add(@event);
    }

    public void Expire()
    {
        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StateSnapshotErrors.InvalidStateTransition(Status, "Expire");

        var @event = new StateSnapshotExpiredEvent(SnapshotId);
        Apply(@event);
        EnsureInvariants();
        _domainEvents.Add(@event);
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(object domainEvent)
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

    // ── Invariants ───────────────────────────────────────────────

    private void EnsureInvariants()
    {
        if (SnapshotId == default)
            throw StateSnapshotErrors.MissingId();

        if (Descriptor == default)
            throw StateSnapshotErrors.MissingDescriptor();
    }
}
