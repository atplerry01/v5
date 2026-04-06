using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public abstract class ExchangeRateService
{
    public abstract ExchangeRate GetRate(Currency from, Currency to);
}
