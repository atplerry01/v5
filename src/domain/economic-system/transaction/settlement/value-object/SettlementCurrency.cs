namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public readonly record struct SettlementCurrency
{
    public string Value { get; }

    public SettlementCurrency(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SettlementCurrency cannot be empty.", nameof(value));
        Value = value.Trim().ToUpperInvariant();
    }

    public static SettlementCurrency From(string value) => new(value);

    public override string ToString() => Value;
}
