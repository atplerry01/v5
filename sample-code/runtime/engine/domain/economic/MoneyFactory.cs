using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Domain.Economic;

namespace Whycespace.Runtime.Engine.Domain.Economic;

/// <summary>
/// Runtime implementation of IMoneyFactory — bridges engine requests to domain Money/Currency.
/// </summary>
public sealed class MoneyFactory : IMoneyFactory
{
    public object CreateMoney(decimal amount, string currencyCode)
    {
        var currency = new Currency(currencyCode);
        return new Money(amount, currency);
    }

    public object CreateCurrency(string currencyCode)
    {
        return new Currency(currencyCode);
    }
}
