using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public sealed class OrderAggregate : AggregateRoot
{
    public OrderId Id { get; private set; }
    public OrderSourceReference SourceReference { get; private set; }
    public OrderDescription Description { get; private set; }
    public OrderStatus Status { get; private set; }

    public static OrderAggregate Create(OrderId id, OrderSourceReference sourceReference, OrderDescription description)
    {
        if (description == default)
            throw OrderErrors.MissingDescription();

        var aggregate = new OrderAggregate();
        if (aggregate.Version >= 0)
            throw OrderErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new OrderCreatedEvent(id, sourceReference, description));
        return aggregate;
    }

    public void Confirm()
    {
        var specification = new CanConfirmOrderSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw OrderErrors.InvalidStateTransition(Status, nameof(Confirm));

        RaiseDomainEvent(new OrderConfirmedEvent(Id));
    }

    public void Complete()
    {
        var specification = new CanCompleteOrderSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw OrderErrors.InvalidStateTransition(Status, nameof(Complete));

        RaiseDomainEvent(new OrderCompletedEvent(Id));
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        var specification = new CanCancelOrderSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw OrderErrors.InvalidStateTransition(Status, nameof(Cancel));

        RaiseDomainEvent(new OrderCancelledEvent(Id, cancelledAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case OrderCreatedEvent e:
                Id = e.OrderId;
                SourceReference = e.SourceReference;
                Description = e.Description;
                Status = OrderStatus.Created;
                break;
            case OrderConfirmedEvent:
                Status = OrderStatus.Confirmed;
                break;
            case OrderCompletedEvent:
                Status = OrderStatus.Completed;
                break;
            case OrderCancelledEvent:
                Status = OrderStatus.Cancelled;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw OrderErrors.MissingId();

        if (SourceReference == default)
            throw OrderErrors.MissingSourceReference();

        if (Description == default)
            throw OrderErrors.MissingDescription();

        if (!Enum.IsDefined(Status))
            throw OrderErrors.InvalidStateTransition(Status, "validate");
    }
}
