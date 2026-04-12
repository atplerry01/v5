namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public sealed class RateImmutabilityService
{
    /// <summary>
    /// Validates that an active rate has not been modified.
    /// Active rates are immutable — no field may change after activation.
    /// </summary>
    public bool IsImmutable(ExchangeRateAggregate rate)
    {
        return rate.Status == ExchangeRateStatus.Active
            || rate.Status == ExchangeRateStatus.Expired;
    }
}
