namespace Whycespace.Domain.BusinessSystem.Inventory.Item;

public sealed class IsActiveSpecification
{
    public bool IsSatisfiedBy(ItemStatus status)
    {
        return status == ItemStatus.Active;
    }
}
