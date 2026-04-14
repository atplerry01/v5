namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public readonly record struct SettlementAmount
{
    public decimal Value { get; }

    public SettlementAmount(decimal value)
    {
        if (value <= 0m)
            throw SettlementErrors.InvalidAmount();
        Value = value;
    }

    public static SettlementAmount From(decimal value) => new(value);

    public override string ToString() => Value.ToString("0.00############");
}
