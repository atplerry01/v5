namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

/// <summary>
/// Identifies the external rail or payment provider that will execute (or has
/// executed) the settlement (e.g., "Bank", "Stripe", "Wise", "ACH"). The
/// domain stores this as an opaque identifier — no protocol logic lives here.
/// </summary>
public readonly record struct SettlementProvider
{
    public string Value { get; }

    public SettlementProvider(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SettlementProvider cannot be empty.", nameof(value));
        Value = value.Trim();
    }

    public static SettlementProvider From(string value) => new(value);

    public override string ToString() => Value;
}
