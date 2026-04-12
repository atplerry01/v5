namespace Whycespace.Domain.BusinessSystem.Marketplace.Order;

public sealed class CanConfirmOrderSpecification
{
    public bool IsSatisfiedBy(OrderStatus status)
    {
        return status == OrderStatus.Created;
    }
}
