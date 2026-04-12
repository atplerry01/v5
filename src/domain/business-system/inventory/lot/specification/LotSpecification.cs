namespace Whycespace.Domain.BusinessSystem.Inventory.Lot;

public sealed class LotSpecification
{
    public bool IsSatisfiedBy(LotAggregate lot)
    {
        return lot.Id != default
            && !string.IsNullOrWhiteSpace(lot.Origin.Value)
            && Enum.IsDefined(lot.Status);
    }
}
