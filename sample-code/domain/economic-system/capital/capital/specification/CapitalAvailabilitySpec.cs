namespace Whycespace.Domain.EconomicSystem.Capital.Capital;

public sealed class CapitalAvailabilitySpec
{
    public bool IsSatisfiedBy(decimal availableBalance, decimal requestedAmount)
    {
        return requestedAmount > 0 && availableBalance >= requestedAmount;
    }
}
