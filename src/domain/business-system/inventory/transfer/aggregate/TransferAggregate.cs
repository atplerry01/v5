namespace Whycespace.Domain.BusinessSystem.Inventory.Transfer;

public sealed class TransferAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public TransferId Id { get; private set; }
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public TransferQuantity Quantity { get; private set; }
    public TransferStatus Status { get; private set; }
    public int Version { get; private set; }

    private TransferAggregate() { }

    public static TransferAggregate Create(
        TransferId id,
        Guid sourceWarehouseId,
        Guid destinationWarehouseId,
        TransferQuantity quantity)
    {
        if (sourceWarehouseId == destinationWarehouseId)
            throw TransferErrors.SameSourceAndDestination();

        var aggregate = new TransferAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new TransferCreatedEvent(id, sourceWarehouseId, destinationWarehouseId, quantity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteTransferSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TransferErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new TransferCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Cancel()
    {
        ValidateBeforeChange();

        var specification = new CanCancelTransferSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw TransferErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new TransferCancelledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(TransferCreatedEvent @event)
    {
        Id = @event.TransferId;
        SourceWarehouseId = @event.SourceWarehouseId;
        DestinationWarehouseId = @event.DestinationWarehouseId;
        Quantity = @event.Quantity;
        Status = TransferStatus.Pending;
        Version++;
    }

    private void Apply(TransferCompletedEvent @event)
    {
        Status = TransferStatus.Completed;
        Version++;
    }

    private void Apply(TransferCancelledEvent @event)
    {
        Status = TransferStatus.Cancelled;
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
            throw TransferErrors.MissingId();

        if (SourceWarehouseId == DestinationWarehouseId)
            throw TransferErrors.SameSourceAndDestination();

        if (!Enum.IsDefined(Status))
            throw TransferErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
