namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public sealed class RecurrenceAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RecurrenceId Id { get; private set; }
    public RecurrencePattern Pattern { get; private set; }
    public RecurrenceStatus Status { get; private set; }
    public int Version { get; private set; }

    private RecurrenceAggregate() { }

    public static RecurrenceAggregate Create(RecurrenceId id, RecurrencePattern pattern)
    {
        var aggregate = new RecurrenceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RecurrenceCreatedEvent(id, pattern);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Terminate()
    {
        ValidateBeforeChange();

        var specification = new CanTerminateRecurrenceSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RecurrenceErrors.InvalidStateTransition(Status, nameof(Terminate));

        var @event = new RecurrenceTerminatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RecurrenceCreatedEvent @event)
    {
        Id = @event.RecurrenceId;
        Pattern = @event.Pattern;
        Status = RecurrenceStatus.Active;
        Version++;
    }

    private void Apply(RecurrenceTerminatedEvent @event)
    {
        Status = RecurrenceStatus.Terminated;
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
            throw RecurrenceErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw RecurrenceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
