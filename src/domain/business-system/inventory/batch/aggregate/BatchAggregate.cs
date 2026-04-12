namespace Whycespace.Domain.BusinessSystem.Inventory.Batch;

public sealed class BatchAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public BatchId Id { get; private set; }
    public BatchStatus Status { get; private set; }
    public int Version { get; private set; }

    private BatchAggregate() { }

    public static BatchAggregate Create(BatchId id)
    {
        var aggregate = new BatchAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new BatchCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Close()
    {
        ValidateBeforeChange();

        var specification = new CanCloseBatchSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BatchErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new BatchClosedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(BatchCreatedEvent @event)
    {
        Id = @event.BatchId;
        Status = BatchStatus.Open;
        Version++;
    }

    private void Apply(BatchClosedEvent @event)
    {
        Status = BatchStatus.Closed;
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
            throw BatchErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw BatchErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
