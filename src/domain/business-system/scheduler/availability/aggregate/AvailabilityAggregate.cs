namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public sealed class AvailabilityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AvailabilityId Id { get; private set; }
    public TimeRange Range { get; private set; }
    public AvailabilityStatus Status { get; private set; }
    public int Version { get; private set; }

    private AvailabilityAggregate() { }

    public static AvailabilityAggregate Create(AvailabilityId id, TimeRange range)
    {
        var aggregate = new AvailabilityAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AvailabilityCreatedEvent(id, range);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendAvailabilitySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AvailabilityErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new AvailabilitySuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reactivate()
    {
        ValidateBeforeChange();

        var specification = new CanReactivateAvailabilitySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AvailabilityErrors.InvalidStateTransition(Status, nameof(Reactivate));

        var @event = new AvailabilityReactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateAvailabilitySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AvailabilityErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new AvailabilityDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AvailabilityCreatedEvent @event)
    {
        Id = @event.AvailabilityId;
        Range = @event.Range;
        Status = AvailabilityStatus.Active;
        Version++;
    }

    private void Apply(AvailabilitySuspendedEvent @event)
    {
        Status = AvailabilityStatus.Suspended;
        Version++;
    }

    private void Apply(AvailabilityReactivatedEvent @event)
    {
        Status = AvailabilityStatus.Active;
        Version++;
    }

    private void Apply(AvailabilityDeactivatedEvent @event)
    {
        Status = AvailabilityStatus.Deactivated;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw AvailabilityErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw AvailabilityErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
