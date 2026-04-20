namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed class CanUpdateSpecification
{
    public bool IsSatisfiedBy(LineItemStatus status) => status != LineItemStatus.Cancelled;
}
