namespace Whycespace.Domain.BusinessSystem.Scheduler.Slot;

public sealed class SlotAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SlotId Id { get; private set; }
    public TimeSlot TimeSlot { get; private set; }
    public SlotStatus Status { get; private set; }
    public int Version { get; private set; }

    private SlotAggregate() { }

    public static SlotAggregate Create(SlotId id, TimeSlot timeSlot)
    {
        var aggregate = new SlotAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SlotCreatedEvent(id, timeSlot);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Cancel()
    {
        ValidateBeforeChange();

        var specification = new CanCancelSlotSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SlotErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new SlotCancelledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SlotCreatedEvent @event)
    {
        Id = @event.SlotId;
        TimeSlot = @event.TimeSlot;
        Status = SlotStatus.Open;
        Version++;
    }

    private void Apply(SlotCancelledEvent @event)
    {
        Status = SlotStatus.Cancelled;
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
            throw SlotErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SlotErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
