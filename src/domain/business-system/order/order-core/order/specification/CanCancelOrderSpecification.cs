namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

// Order may be cancelled from Created or Confirmed, never after Completed or
// from an already-Cancelled state.
public sealed class CanCancelOrderSpecification
{
    public bool IsSatisfiedBy(OrderStatus status)
        => status is OrderStatus.Created or OrderStatus.Confirmed;
}
