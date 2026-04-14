using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Vault.Metrics;

/// <summary>
/// Immutable snapshot of a vault account's capital distribution across the
/// four buckets tracked by the doctrine:
///   Total    = total capital in the vault across all slices
///   Free     = capital in Slice1, available for funding / exit
///   Locked   = capital reserved but not yet invested
///   Invested = capital committed to Slice2+ positions
///
/// Invariant (enforced on construction): Total = Free + Locked + Invested.
/// No metrics events are emitted — metrics are a view of vault state, not a
/// BC. Updates happen via business methods on VaultAccountAggregate which
/// rebuild the VaultMetrics value and store it.
/// </summary>
public readonly record struct VaultMetrics
{
    public Amount Total { get; }
    public Amount Free { get; }
    public Amount Locked { get; }
    public Amount Invested { get; }

    public VaultMetrics(Amount total, Amount free, Amount locked, Amount invested)
    {
        if (total.Value < 0m)
            throw new ArgumentException("VaultMetrics.Total cannot be negative.", nameof(total));

        if (free.Value < 0m)
            throw new ArgumentException("VaultMetrics.Free cannot be negative.", nameof(free));

        if (locked.Value < 0m)
            throw new ArgumentException("VaultMetrics.Locked cannot be negative.", nameof(locked));

        if (invested.Value < 0m)
            throw new ArgumentException("VaultMetrics.Invested cannot be negative.", nameof(invested));

        if (total.Value != free.Value + locked.Value + invested.Value)
            throw new ArgumentException(
                $"VaultMetrics invariant violated: Total ({total.Value:F2}) != " +
                $"Free ({free.Value:F2}) + Locked ({locked.Value:F2}) + Invested ({invested.Value:F2}).");

        Total = total;
        Free = free;
        Locked = locked;
        Invested = invested;
    }

    public static VaultMetrics Zero() =>
        new(new Amount(0m), new Amount(0m), new Amount(0m), new Amount(0m));

    /// <summary>
    /// Credit Slice1 (funding). Total and Free grow by the same amount.
    /// </summary>
    public VaultMetrics WithFunding(Amount amount) =>
        new(
            new Amount(Total.Value + amount.Value),
            new Amount(Free.Value + amount.Value),
            Locked,
            Invested);

    /// <summary>
    /// Move capital from Slice1 (Free) into Slice2 (Invested).
    /// Total unchanged. Free shrinks, Invested grows.
    /// </summary>
    public VaultMetrics WithInvestment(Amount amount) =>
        new(
            Total,
            new Amount(Free.Value - amount.Value),
            Locked,
            new Amount(Invested.Value + amount.Value));

    /// <summary>
    /// Debit Slice1 (payout). Total and Free shrink by the same amount.
    /// </summary>
    public VaultMetrics WithDebit(Amount amount) =>
        new(
            new Amount(Total.Value - amount.Value),
            new Amount(Free.Value - amount.Value),
            Locked,
            Invested);
}
