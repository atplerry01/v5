namespace Whycespace.Domain.BusinessSystem.Marketplace.Order;

public sealed class IsConfirmedOrderSpecification
{
    public bool IsSatisfiedBy(OrderStatus status)
    {
        return status == OrderStatus.Confirmed;
    }
}
