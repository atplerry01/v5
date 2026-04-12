namespace Whycespace.Domain.BusinessSystem.Inventory.Lot;

public sealed class CanSealLotSpecification
{
    public bool IsSatisfiedBy(LotStatus status)
    {
        return status == LotStatus.Active;
    }
}
