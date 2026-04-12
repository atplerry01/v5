namespace Whycespace.Domain.BusinessSystem.Document.Record;

public sealed class RecordAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RecordId Id { get; private set; }
    public RecordStatus Status { get; private set; }
    public int Version { get; private set; }

    private RecordAggregate() { }

    public static RecordAggregate Create(RecordId id)
    {
        var aggregate = new RecordAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RecordCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Lock()
    {
        ValidateBeforeChange();

        var specification = new CanLockSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RecordErrors.InvalidStateTransition(Status, nameof(Lock));

        var @event = new RecordLockedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveRecordSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RecordErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new RecordArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RecordCreatedEvent @event)
    {
        Id = @event.RecordId;
        Status = RecordStatus.Active;
        Version++;
    }

    private void Apply(RecordLockedEvent @event)
    {
        Status = RecordStatus.Locked;
        Version++;
    }

    private void Apply(RecordArchivedEvent @event)
    {
        Status = RecordStatus.Archived;
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
            throw RecordErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw RecordErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
