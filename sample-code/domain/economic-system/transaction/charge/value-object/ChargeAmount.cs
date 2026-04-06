namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeAmount
{
    public decimal Value { get; }

    public ChargeAmount(decimal value)
    {
        if (value <= 0)
            throw new DomainException("CHARGE.INVALID_AMOUNT", $"Charge amount must be greater than zero. Got: {value}");

        Value = value;
    }

    public static implicit operator decimal(ChargeAmount amount) => amount.Value;
}
