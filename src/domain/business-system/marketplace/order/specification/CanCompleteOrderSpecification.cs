namespace Whycespace.Domain.BusinessSystem.Marketplace.Order;

public sealed class CanCompleteOrderSpecification
{
    public bool IsSatisfiedBy(OrderStatus status)
    {
        return status == OrderStatus.Confirmed;
    }
}
