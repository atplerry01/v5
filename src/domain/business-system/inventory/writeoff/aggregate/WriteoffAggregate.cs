namespace Whycespace.Domain.BusinessSystem.Inventory.Writeoff;

public sealed class WriteoffAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public WriteoffId Id { get; private set; }
    public WriteoffReference Reference { get; private set; }
    public WriteoffQuantity Quantity { get; private set; }
    public WriteoffReason Reason { get; private set; }
    public WriteoffStatus Status { get; private set; }
    public int Version { get; private set; }

    private WriteoffAggregate() { }

    public static WriteoffAggregate Create(
        WriteoffId id,
        WriteoffReference reference,
        WriteoffQuantity quantity,
        WriteoffReason reason)
    {
        var aggregate = new WriteoffAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new WriteoffCreatedEvent(id, reference, quantity, reason);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Confirm()
    {
        ValidateBeforeChange();

        var specification = new CanConfirmWriteoffSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw WriteoffErrors.InvalidStateTransition(Status, nameof(Confirm));

        var @event = new WriteoffConfirmedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(WriteoffCreatedEvent @event)
    {
        Id = @event.WriteoffId;
        Reference = @event.Reference;
        Quantity = @event.Quantity;
        Reason = @event.Reason;
        Status = WriteoffStatus.Pending;
        Version++;
    }

    private void Apply(WriteoffConfirmedEvent @event)
    {
        Status = WriteoffStatus.Confirmed;
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
            throw WriteoffErrors.MissingId();

        if (Reference.Value == Guid.Empty)
            throw WriteoffErrors.MissingReference();

        if (!Enum.IsDefined(Status))
            throw WriteoffErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
