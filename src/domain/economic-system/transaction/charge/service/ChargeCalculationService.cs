using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed class ChargeCalculationService
{
    public Amount CalculateFixedCharge(Amount fixedFee)
    {
        if (fixedFee.Value <= 0m) throw ChargeErrors.InvalidBaseAmount();
        return fixedFee;
    }

    public Amount CalculatePercentageCharge(Amount baseAmount, decimal rate)
    {
        if (baseAmount.Value <= 0m) throw ChargeErrors.InvalidBaseAmount();
        var calculated = Math.Round(baseAmount.Value * rate / 100m, 2, MidpointRounding.ToEven);
        return new Amount(calculated);
    }
}
