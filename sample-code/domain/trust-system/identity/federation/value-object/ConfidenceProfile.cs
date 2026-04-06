namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Tracks confidence evolution for a federation link over time.
/// Replaces static ConfidenceLevel with a profile that records adjustments.
///
/// Rules:
///   - Confidence increases with verified credentials
///   - Confidence decreases with anomalies
///   - All adjustments are recorded for auditability
///   - Bounded [0, 1] at all times
/// </summary>
public sealed class ConfidenceProfile
{
    public ConfidenceLevel CurrentConfidence { get; private set; }
    public ConfidenceLevel InitialConfidence { get; }
    public DateTimeOffset LastUpdatedAt { get; private set; }

    private readonly List<ConfidenceAdjustment> _history = [];
    public IReadOnlyList<ConfidenceAdjustment> AdjustmentHistory => _history.AsReadOnly();

    public ConfidenceProfile(ConfidenceLevel initial, DateTimeOffset createdAt)
    {
        Guard.AgainstNull(initial);
        InitialConfidence = initial;
        CurrentConfidence = initial;
        LastUpdatedAt = createdAt;
    }

    /// <summary>
    /// Increase confidence (e.g. credential verified).
    /// </summary>
    public void IncreaseConfidence(decimal amount, string reason, DateTimeOffset adjustedAt)
    {
        Guard.AgainstEmpty(reason);
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Increase amount must be positive.");

        var previous = CurrentConfidence.Value;
        var newValue = Math.Min(previous + amount, 1m);
        CurrentConfidence = new ConfidenceLevel(newValue);
        LastUpdatedAt = adjustedAt;

        _history.Add(new ConfidenceAdjustment(
            previous, newValue, amount, reason, ConfidenceAdjustmentDirection.Increase, adjustedAt));
    }

    /// <summary>
    /// Decrease confidence (e.g. anomaly detected).
    /// </summary>
    public void DecreaseConfidence(decimal amount, string reason, DateTimeOffset adjustedAt)
    {
        Guard.AgainstEmpty(reason);
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Decrease amount must be positive.");

        var previous = CurrentConfidence.Value;
        var newValue = Math.Max(previous - amount, 0m);
        CurrentConfidence = new ConfidenceLevel(newValue);
        LastUpdatedAt = adjustedAt;

        _history.Add(new ConfidenceAdjustment(
            previous, newValue, -amount, reason, ConfidenceAdjustmentDirection.Decrease, adjustedAt));
    }
}

public sealed record ConfidenceAdjustment(
    decimal PreviousValue,
    decimal NewValue,
    decimal Delta,
    string Reason,
    ConfidenceAdjustmentDirection Direction,
    DateTimeOffset AdjustedAt);

public enum ConfidenceAdjustmentDirection
{
    Increase,
    Decrease
}
