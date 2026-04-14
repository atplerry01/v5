namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public readonly record struct SettlementReferenceId
{
    public string Value { get; }

    public SettlementReferenceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SettlementReferenceId cannot be empty.", nameof(value));
        Value = value.Trim();
    }

    public static SettlementReferenceId From(string value) => new(value);

    public override string ToString() => Value;
}
