namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record CreditAmount
{
    public Amount Value { get; }

    public CreditAmount(Amount value)
    {
        if (value.IsNegative)
            throw new ArgumentException("CreditAmount cannot be negative.", nameof(value));

        Value = value;
    }

    public static CreditAmount Zero => new(Amount.Zero);

    public override string ToString() => Value.ToString();
}
