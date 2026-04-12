namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public sealed class CanDeactivateReplenishmentSpecification
{
    public bool IsSatisfiedBy(ReplenishmentStatus status)
    {
        return status == ReplenishmentStatus.Active || status == ReplenishmentStatus.Suspended;
    }
}
