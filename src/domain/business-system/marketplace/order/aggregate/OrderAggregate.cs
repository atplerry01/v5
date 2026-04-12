namespace Whycespace.Domain.BusinessSystem.Marketplace.Order;

public sealed class OrderAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public OrderId Id { get; private set; }
    public OrderSourceReference SourceReference { get; private set; }
    public string OrderDescription { get; private set; }
    public OrderStatus Status { get; private set; }
    public int Version { get; private set; }

    private OrderAggregate() { }

    public static OrderAggregate Create(OrderId id, OrderSourceReference sourceReference, string orderDescription)
    {
        if (string.IsNullOrWhiteSpace(orderDescription))
            throw new ArgumentException("Order description must not be empty.", nameof(orderDescription));

        var aggregate = new OrderAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new OrderCreatedEvent(id, sourceReference, orderDescription);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Confirm()
    {
        ValidateBeforeChange();

        var specification = new CanConfirmOrderSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw OrderErrors.InvalidStateTransition(Status, nameof(Confirm));

        var @event = new OrderConfirmedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteOrderSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw OrderErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new OrderCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(OrderCreatedEvent @event)
    {
        Id = @event.OrderId;
        SourceReference = @event.SourceReference;
        OrderDescription = @event.OrderDescription;
        Status = OrderStatus.Created;
        Version++;
    }

    private void Apply(OrderConfirmedEvent @event)
    {
        Status = OrderStatus.Confirmed;
        Version++;
    }

    private void Apply(OrderCompletedEvent @event)
    {
        Status = OrderStatus.Completed;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw OrderErrors.MissingId();

        if (SourceReference == default)
            throw OrderErrors.MissingSourceReference();

        if (!Enum.IsDefined(Status))
            throw OrderErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
