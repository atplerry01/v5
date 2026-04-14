namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public readonly record struct SettlementId
{
    public Guid Value { get; }

    public SettlementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SettlementId cannot be empty.", nameof(value));
        Value = value;
    }

    public static SettlementId From(Guid value) => new(value);
}
