namespace Whycespace.Domain.BusinessSystem.Inventory.Movement;

public sealed class MovementAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public MovementId Id { get; private set; }
    public MovementSourceId SourceId { get; private set; }
    public MovementTargetId TargetId { get; private set; }
    public MovementQuantity Quantity { get; private set; }
    public MovementStatus Status { get; private set; }
    public int Version { get; private set; }

    private MovementAggregate() { }

    public static MovementAggregate Create(MovementId id, MovementSourceId sourceId, MovementTargetId targetId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Movement quantity must be greater than zero.", nameof(quantity));

        var aggregate = new MovementAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new MovementCreatedEvent(id, sourceId, targetId, quantity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Confirm()
    {
        ValidateBeforeChange();

        var specification = new CanConfirmSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MovementErrors.InvalidStateTransition(Status, nameof(Confirm));

        var @event = new MovementConfirmedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Cancel()
    {
        ValidateBeforeChange();

        var specification = new CanCancelSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MovementErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new MovementCancelledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(MovementCreatedEvent @event)
    {
        Id = @event.MovementId;
        SourceId = @event.SourceId;
        TargetId = @event.TargetId;
        Quantity = new MovementQuantity(@event.Quantity);
        Status = MovementStatus.Pending;
        Version++;
    }

    private void Apply(MovementConfirmedEvent @event)
    {
        Status = MovementStatus.Confirmed;
        Version++;
    }

    private void Apply(MovementCancelledEvent @event)
    {
        Status = MovementStatus.Cancelled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw MovementErrors.MissingId();

        if (SourceId == default)
            throw MovementErrors.MissingSourceId();

        if (TargetId == default)
            throw MovementErrors.MissingTargetId();

        if (Quantity.Value <= 0)
            throw MovementErrors.InvalidQuantity();

        if (!Enum.IsDefined(Status))
            throw MovementErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
