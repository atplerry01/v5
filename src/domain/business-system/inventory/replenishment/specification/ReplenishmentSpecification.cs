namespace Whycespace.Domain.BusinessSystem.Inventory.Replenishment;

public sealed class ReplenishmentSpecification
{
    public bool IsSatisfiedBy(ReplenishmentAggregate replenishment)
    {
        return replenishment.Id != default
            && replenishment.Threshold.Value >= 0
            && replenishment.RestockQuantity.Value > 0
            && Enum.IsDefined(replenishment.Status);
    }
}
