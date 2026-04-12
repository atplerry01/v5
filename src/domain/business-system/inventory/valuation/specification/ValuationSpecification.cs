namespace Whycespace.Domain.BusinessSystem.Inventory.Valuation;

public sealed class ValuationSpecification
{
    public bool IsSatisfiedBy(ValuationAggregate valuation)
    {
        return valuation.Id != default
            && Enum.IsDefined(valuation.Method)
            && Enum.IsDefined(valuation.Status);
    }
}
