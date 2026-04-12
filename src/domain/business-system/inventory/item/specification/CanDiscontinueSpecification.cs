namespace Whycespace.Domain.BusinessSystem.Inventory.Item;

public sealed class CanDiscontinueSpecification
{
    public bool IsSatisfiedBy(ItemStatus status)
    {
        return status == ItemStatus.Active;
    }
}
