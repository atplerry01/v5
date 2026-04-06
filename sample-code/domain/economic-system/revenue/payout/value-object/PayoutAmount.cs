namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public sealed record PayoutAmount
{
    public decimal Value { get; }

    public PayoutAmount(decimal value)
    {
        if (value <= 0)
            throw new PayoutException("Payout amount must be positive.");

        Value = value;
    }

    public override string ToString() => Value.ToString("F2");
}
