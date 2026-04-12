namespace Whycespace.Domain.BusinessSystem.Inventory.Valuation;

public sealed class CanReactivateValuationSpecification
{
    public bool IsSatisfiedBy(ValuationStatus status)
    {
        return status == ValuationStatus.Suspended;
    }
}
