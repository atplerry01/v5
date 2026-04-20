namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public sealed class CanCancelSpecification
{
    public bool IsSatisfiedBy(LineItemStatus status) => status != LineItemStatus.Cancelled;
}
