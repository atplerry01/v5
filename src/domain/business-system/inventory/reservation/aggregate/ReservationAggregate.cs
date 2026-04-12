namespace Whycespace.Domain.BusinessSystem.Inventory.Reservation;

public sealed class ReservationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ReservationId Id { get; private set; }
    public ReservationItemId ItemId { get; private set; }
    public ReservedQuantity Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public int Version { get; private set; }

    private ReservationAggregate() { }

    public static ReservationAggregate Create(ReservationId id, ReservationItemId itemId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reserved quantity must be greater than zero.", nameof(quantity));

        var aggregate = new ReservationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ReservationCreatedEvent(id, itemId, quantity);
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
            throw ReservationErrors.InvalidStateTransition(Status, nameof(Confirm));

        var @event = new ReservationConfirmedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Release()
    {
        ValidateBeforeChange();

        var specification = new CanReleaseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReservationErrors.InvalidStateTransition(Status, nameof(Release));

        var @event = new ReservationReleasedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ReservationCreatedEvent @event)
    {
        Id = @event.ReservationId;
        ItemId = @event.ItemId;
        Quantity = new ReservedQuantity(@event.Quantity);
        Status = ReservationStatus.Reserved;
        Version++;
    }

    private void Apply(ReservationConfirmedEvent @event)
    {
        Status = ReservationStatus.Confirmed;
        Version++;
    }

    private void Apply(ReservationReleasedEvent @event)
    {
        Status = ReservationStatus.Released;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ReservationErrors.MissingId();

        if (ItemId == default)
            throw ReservationErrors.MissingItemId();

        if (Quantity.Value <= 0)
            throw ReservationErrors.InvalidQuantity();

        if (!Enum.IsDefined(Status))
            throw ReservationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
