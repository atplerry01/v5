namespace Whycespace.Domain.BusinessSystem.Inventory.Valuation;

public sealed class CanSuspendValuationSpecification
{
    public bool IsSatisfiedBy(ValuationStatus status)
    {
        return status == ValuationStatus.Active;
    }
}
