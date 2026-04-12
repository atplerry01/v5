namespace Whycespace.Domain.BusinessSystem.Logistic.Shipment;

public sealed class ShipmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ShipmentId Id { get; private set; }
    public Origin Origin { get; private set; }
    public Destination Destination { get; private set; }
    public ItemReference ItemReference { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public int Version { get; private set; }

    private ShipmentAggregate() { }

    public static ShipmentAggregate Create(
        ShipmentId id,
        Origin origin,
        Destination destination,
        ItemReference itemReference)
    {
        var aggregate = new ShipmentAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ShipmentCreatedEvent(id, origin, destination, itemReference);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Pack()
    {
        ValidateBeforeChange();

        var specification = new CanPackSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ShipmentErrors.InvalidStateTransition(Status, nameof(Pack));

        var @event = new ShipmentPackedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Dispatch()
    {
        ValidateBeforeChange();

        var specification = new CanDispatchSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ShipmentErrors.InvalidStateTransition(Status, nameof(Dispatch));

        var @event = new ShipmentDispatchedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkInTransit()
    {
        ValidateBeforeChange();

        var specification = new CanMarkInTransitSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ShipmentErrors.InvalidStateTransition(Status, nameof(MarkInTransit));

        var @event = new ShipmentInTransitEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deliver()
    {
        ValidateBeforeChange();

        var specification = new CanDeliverSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ShipmentErrors.InvalidStateTransition(Status, nameof(Deliver));

        var @event = new ShipmentDeliveredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ShipmentCreatedEvent @event)
    {
        Id = @event.ShipmentId;
        Origin = @event.Origin;
        Destination = @event.Destination;
        ItemReference = @event.ItemReference;
        Status = ShipmentStatus.Created;
        Version++;
    }

    private void Apply(ShipmentPackedEvent @event)
    {
        Status = ShipmentStatus.Packed;
        Version++;
    }

    private void Apply(ShipmentDispatchedEvent @event)
    {
        Status = ShipmentStatus.Dispatched;
        Version++;
    }

    private void Apply(ShipmentInTransitEvent @event)
    {
        Status = ShipmentStatus.InTransit;
        Version++;
    }

    private void Apply(ShipmentDeliveredEvent @event)
    {
        Status = ShipmentStatus.Delivered;
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
            throw ShipmentErrors.MissingId();

        if (Origin == default)
            throw ShipmentErrors.OriginRequired();

        if (Destination == default)
            throw ShipmentErrors.DestinationRequired();

        if (ItemReference == default)
            throw ShipmentErrors.ItemReferenceRequired();

        if (!Enum.IsDefined(Status))
            throw ShipmentErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
