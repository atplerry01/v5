namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed class RevenueEligibilitySpec
{
    public bool IsSatisfiedBy(decimal amount) => amount > 0;
}
