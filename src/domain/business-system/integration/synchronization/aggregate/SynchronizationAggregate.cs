namespace Whycespace.Domain.BusinessSystem.Integration.Synchronization;

public sealed class SynchronizationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SynchronizationId Id { get; private set; }
    public SyncPolicyId PolicyId { get; private set; }
    public SynchronizationStatus Status { get; private set; }
    public int Version { get; private set; }

    private SynchronizationAggregate() { }

    public static SynchronizationAggregate Create(SynchronizationId id, SyncPolicyId policyId)
    {
        var aggregate = new SynchronizationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SynchronizationCreatedEvent(id, policyId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void MarkPending()
    {
        ValidateBeforeChange();

        var specification = new CanMarkPendingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SynchronizationErrors.InvalidStateTransition(Status, nameof(MarkPending));

        var @event = new SynchronizationPendingEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Confirm()
    {
        ValidateBeforeChange();

        var specification = new CanConfirmSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SynchronizationErrors.InvalidStateTransition(Status, nameof(Confirm));

        var @event = new SynchronizationConfirmedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SynchronizationCreatedEvent @event)
    {
        Id = @event.SynchronizationId;
        PolicyId = @event.PolicyId;
        Status = SynchronizationStatus.Defined;
        Version++;
    }

    private void Apply(SynchronizationPendingEvent @event)
    {
        Status = SynchronizationStatus.Pending;
        Version++;
    }

    private void Apply(SynchronizationConfirmedEvent @event)
    {
        Status = SynchronizationStatus.Confirmed;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SynchronizationErrors.MissingId();

        if (PolicyId == default)
            throw SynchronizationErrors.MissingPolicyId();

        if (!Enum.IsDefined(Status))
            throw SynchronizationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
