using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public sealed class StaleRateException : DomainException
{
    public StaleRateException(Guid rateId, DateTimeOffset expiredAt)
        : base("STALE_FX_RATE", $"FX rate {rateId} expired at {expiredAt}.") { }
}

public sealed class CurrencyPairMismatchException : DomainException
{
    public CurrencyPairMismatchException(string expected, string actual)
        : base("CURRENCY_PAIR_MISMATCH", $"Expected pair '{expected}', got '{actual}'.") { }
}
