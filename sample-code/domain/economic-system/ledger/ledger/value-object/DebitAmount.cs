namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record DebitAmount
{
    public Amount Value { get; }

    public DebitAmount(Amount value)
    {
        if (value.IsNegative)
            throw new ArgumentException("DebitAmount cannot be negative.", nameof(value));

        Value = value;
    }

    public static DebitAmount Zero => new(Amount.Zero);

    public override string ToString() => Value.ToString();
}
