namespace Whycespace.Domain.BusinessSystem.Inventory.Valuation;

public sealed class CanDeactivateValuationSpecification
{
    public bool IsSatisfiedBy(ValuationStatus status)
    {
        return status == ValuationStatus.Active || status == ValuationStatus.Suspended;
    }
}
