using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Capital.Capital;

public sealed class MonetaryPrecisionSpec
{
    public bool IsSatisfiedBy(Money money)
    {
        return !money.IsNegative
            && decimal.Round(money.Amount, money.Currency.DecimalPlaces) == money.Amount;
    }
}
