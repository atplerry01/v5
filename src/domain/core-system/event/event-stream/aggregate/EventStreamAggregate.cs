namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public sealed class EventStreamAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EventStreamId Id { get; private set; }
    public StreamDescriptor Descriptor { get; private set; }
    public EventStreamStatus Status { get; private set; }
    public int Version { get; private set; }

    private EventStreamAggregate() { }

    public static EventStreamAggregate Open(EventStreamId id, StreamDescriptor descriptor)
    {
        var aggregate = new EventStreamAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new EventStreamOpenedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Seal()
    {
        ValidateBeforeChange();

        var specification = new CanSealSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventStreamErrors.InvalidStateTransition(Status, nameof(Seal));

        var @event = new EventStreamSealedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EventStreamErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new EventStreamArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EventStreamOpenedEvent @event)
    {
        Id = @event.EventStreamId;
        Descriptor = @event.Descriptor;
        Status = EventStreamStatus.Open;
        Version++;
    }

    private void Apply(EventStreamSealedEvent @event)
    {
        Status = EventStreamStatus.Sealed;
        Version++;
    }

    private void Apply(EventStreamArchivedEvent @event)
    {
        Status = EventStreamStatus.Archived;
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
            throw EventStreamErrors.MissingId();

        if (Descriptor == default)
            throw EventStreamErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw EventStreamErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
