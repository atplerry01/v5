namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public sealed class CanCompleteOrderSpecification
{
    public bool IsSatisfiedBy(OrderStatus status)
    {
        return status == OrderStatus.Confirmed;
    }
}
