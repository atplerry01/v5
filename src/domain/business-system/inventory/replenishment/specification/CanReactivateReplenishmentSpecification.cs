namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public sealed class CanReactivateReplenishmentSpecification
{
    public bool IsSatisfiedBy(ReplenishmentStatus status)
    {
        return status == ReplenishmentStatus.Suspended;
    }
}
