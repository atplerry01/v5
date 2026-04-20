using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed class LineItemAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public LineItemId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public LineItemSubjectRef Subject { get; private set; }
    public LineQuantity Quantity { get; private set; }
    public LineItemStatus Status { get; private set; }
    public int Version { get; private set; }

    private LineItemAggregate() { }

    public static LineItemAggregate Create(
        LineItemId id,
        OrderRef order,
        LineItemSubjectRef subject,
        LineQuantity quantity)
    {
        var aggregate = new LineItemAggregate();

        var @event = new LineItemCreatedEvent(id, order, subject, quantity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void UpdateQuantity(LineQuantity quantity)
    {
        var specification = new CanUpdateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LineItemErrors.CancelledImmutable(Id);

        var @event = new LineItemUpdatedEvent(Id, quantity);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Cancel()
    {
        var specification = new CanCancelSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LineItemErrors.InvalidStateTransition(Status, nameof(Cancel));

        var @event = new LineItemCancelledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(LineItemCreatedEvent @event)
    {
        Id = @event.LineItemId;
        Order = @event.Order;
        Subject = @event.Subject;
        Quantity = @event.Quantity;
        Status = LineItemStatus.Requested;
        Version++;
    }

    private void Apply(LineItemUpdatedEvent @event)
    {
        Quantity = @event.Quantity;
        Version++;
    }

    private void Apply(LineItemCancelledEvent @event)
    {
        Status = LineItemStatus.Cancelled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw LineItemErrors.MissingId();

        if (Order == default)
            throw LineItemErrors.MissingOrderRef();

        if (!Enum.IsDefined(Status))
            throw LineItemErrors.InvalidStateTransition(Status, "validate");
    }
}
