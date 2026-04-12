namespace Whycespace.Domain.BusinessSystem.Scheduler.Calendar;

public sealed class CalendarAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CalendarId Id { get; private set; }
    public CalendarStatus Status { get; private set; }
    public int Version { get; private set; }

    private CalendarAggregate() { }

    public static CalendarAggregate Create(CalendarId id)
    {
        var aggregate = new CalendarAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CalendarCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveCalendarSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CalendarErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new CalendarArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CalendarCreatedEvent @event)
    {
        Id = @event.CalendarId;
        Status = CalendarStatus.Active;
        Version++;
    }

    private void Apply(CalendarArchivedEvent @event)
    {
        Status = CalendarStatus.Archived;
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
            throw CalendarErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw CalendarErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
