namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

/// <summary>
/// Deterministic reference to the internal source that triggered the settlement
/// (ledger entry id, transaction id, or capital movement id). A settlement MUST
/// carry a non-empty source reference — this preserves the audit chain from
/// internal truth (ledger/capital) to external execution.
/// </summary>
public readonly record struct SettlementSourceReference
{
    public string Value { get; }

    public SettlementSourceReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw SettlementErrors.MissingSourceReference();
        Value = value.Trim();
    }

    public static SettlementSourceReference From(string value) => new(value);

    public override string ToString() => Value;
}
