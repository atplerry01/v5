namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public sealed class CanConfirmOrderSpecification
{
    public bool IsSatisfiedBy(OrderStatus status)
    {
        return status == OrderStatus.Created;
    }
}
