namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public sealed class IsConfirmedOrderSpecification
{
    public bool IsSatisfiedBy(OrderStatus status)
    {
        return status == OrderStatus.Confirmed;
    }
}
