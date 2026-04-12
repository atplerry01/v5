namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public sealed class CanSuspendReplenishmentSpecification
{
    public bool IsSatisfiedBy(ReplenishmentStatus status)
    {
        return status == ReplenishmentStatus.Active;
    }
}
