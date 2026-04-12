namespace Whycespace.Domain.BusinessSystem.Scheduler.Schedule;

public sealed class ScheduleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ScheduleId Id { get; private set; }
    public ScheduleStatus Status { get; private set; }
    public int Version { get; private set; }

    private ScheduleAggregate() { }

    public static ScheduleAggregate Create(ScheduleId id)
    {
        var aggregate = new ScheduleAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ScheduleCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendScheduleSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ScheduleErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new ScheduleSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reactivate()
    {
        ValidateBeforeChange();

        var specification = new CanReactivateScheduleSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ScheduleErrors.InvalidStateTransition(Status, nameof(Reactivate));

        var @event = new ScheduleReactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateScheduleSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ScheduleErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new ScheduleDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ScheduleCreatedEvent @event)
    {
        Id = @event.ScheduleId;
        Status = ScheduleStatus.Active;
        Version++;
    }

    private void Apply(ScheduleSuspendedEvent @event)
    {
        Status = ScheduleStatus.Suspended;
        Version++;
    }

    private void Apply(ScheduleReactivatedEvent @event)
    {
        Status = ScheduleStatus.Active;
        Version++;
    }

    private void Apply(ScheduleDeactivatedEvent @event)
    {
        Status = ScheduleStatus.Deactivated;
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
            throw ScheduleErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ScheduleErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
