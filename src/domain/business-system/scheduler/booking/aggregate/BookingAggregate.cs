namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public sealed class BookingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public BookingId Id { get; private set; }
    public BookingTimeRange TimeRange { get; private set; }
    public BookingStatus Status { get; private set; }
    public int Version { get; private set; }

    private BookingAggregate() { }

    public static BookingAggregate Create(BookingId id, BookingTimeRange timeRange)
    {
        var aggregate = new BookingAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new BookingCreatedEvent(id, timeRange);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Confirm()
    {
        ValidateBeforeChange();

        var specification = new CanConfirmBookingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BookingErrors.InvalidStateTransition(Status, nameof(Confirm));

        var @event = new BookingConfirmedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Cancel()
    {
        ValidateBeforeChange();

        var specification = new CanCancelBookingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BookingErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new BookingCancelledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(BookingCreatedEvent @event)
    {
        Id = @event.BookingId;
        TimeRange = @event.TimeRange;
        Status = BookingStatus.Pending;
        Version++;
    }

    private void Apply(BookingConfirmedEvent @event)
    {
        Status = BookingStatus.Confirmed;
        Version++;
    }

    private void Apply(BookingCancelledEvent @event)
    {
        Status = BookingStatus.Cancelled;
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
            throw BookingErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw BookingErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
