namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

/// <summary>
/// Stable, deterministic key for payout retry de-duplication. Required by
/// Phase 3 T3.4. Derived upstream as
/// <c>payout|{distributionId}|{spvId}</c> by IIdGenerator so retries
/// converge on the same PayoutAggregate instance and do not double-emit.
/// </summary>
public readonly record struct PayoutIdempotencyKey
{
    public string Value { get; }

    public PayoutIdempotencyKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PayoutIdempotencyKey cannot be empty.", nameof(value));
        Value = value;
    }

    public static PayoutIdempotencyKey From(string value) => new(value);

    public override string ToString() => Value;
}
