using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed class LineItemAggregate : AggregateRoot
{
    public LineItemId Id { get; private set; }
    public OrderRef Order { get; private set; }
    public LineItemSubjectRef Subject { get; private set; }
    public LineQuantity Quantity { get; private set; }
    public LineItemStatus Status { get; private set; }

    public static LineItemAggregate Create(
        LineItemId id,
        OrderRef order,
        LineItemSubjectRef subject,
        LineQuantity quantity)
    {
        var aggregate = new LineItemAggregate();
        if (aggregate.Version >= 0)
            throw LineItemErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new LineItemCreatedEvent(id, order, subject, quantity));
        return aggregate;
    }

    public void UpdateQuantity(LineQuantity quantity)
    {
        var specification = new CanUpdateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LineItemErrors.CancelledImmutable(Id);

        RaiseDomainEvent(new LineItemUpdatedEvent(Id, quantity));
    }

    public void Cancel()
    {
        var specification = new CanCancelSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LineItemErrors.InvalidStateTransition(Status, nameof(Cancel));

        RaiseDomainEvent(new LineItemCancelledEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LineItemCreatedEvent e:
                Id = e.LineItemId;
                Order = e.Order;
                Subject = e.Subject;
                Quantity = e.Quantity;
                Status = LineItemStatus.Requested;
                break;
            case LineItemUpdatedEvent e:
                Quantity = e.Quantity;
                break;
            case LineItemCancelledEvent:
                Status = LineItemStatus.Cancelled;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw LineItemErrors.MissingId();

        if (Order == default)
            throw LineItemErrors.MissingOrderRef();

        if (Subject == default)
            throw LineItemErrors.MissingSubject();

        if (Quantity == default)
            throw LineItemErrors.MissingQuantity();

        if (!Enum.IsDefined(Status))
            throw LineItemErrors.InvalidStateTransition(Status, "validate");
    }
}
