namespace Whycespace.Domain.CoreSystem.Temporal.EffectivePeriod;

public sealed record EffectivePeriod
{
    public DateTimeOffset? EffectiveFrom { get; }
    public DateTimeOffset? EffectiveTo { get; }

    public EffectivePeriod(DateTimeOffset? effectiveFrom = null, DateTimeOffset? effectiveTo = null)
    {
        var utcFrom = effectiveFrom?.ToUniversalTime();
        var utcTo = effectiveTo?.ToUniversalTime();

        if (utcFrom.HasValue && utcTo.HasValue && utcTo.Value <= utcFrom.Value)
            throw EffectivePeriodErrors.ToMustFollowFrom(utcFrom.Value, utcTo.Value);

        EffectiveFrom = utcFrom;
        EffectiveTo = utcTo;
    }

    // No time boundary — always active.
    public static EffectivePeriod Always => new();

    public bool IsActive(DateTimeOffset at)
    {
        var utc = at.ToUniversalTime();
        return (!EffectiveFrom.HasValue || utc >= EffectiveFrom.Value) &&
               (!EffectiveTo.HasValue || utc < EffectiveTo.Value);
    }

    public bool HasStarted(DateTimeOffset at) =>
        !EffectiveFrom.HasValue || at.ToUniversalTime() >= EffectiveFrom.Value;

    public bool HasExpired(DateTimeOffset at) =>
        EffectiveTo.HasValue && at.ToUniversalTime() >= EffectiveTo.Value;

    public bool Overlaps(EffectivePeriod other)
    {
        var thisFrom = EffectiveFrom ?? DateTimeOffset.MinValue;
        var thisTo = EffectiveTo ?? DateTimeOffset.MaxValue;
        var otherFrom = other.EffectiveFrom ?? DateTimeOffset.MinValue;
        var otherTo = other.EffectiveTo ?? DateTimeOffset.MaxValue;
        return thisFrom < otherTo && thisTo > otherFrom;
    }

    public override string ToString()
    {
        var from = EffectiveFrom.HasValue ? EffectiveFrom.Value.ToString("O") : "−∞";
        var to = EffectiveTo.HasValue ? EffectiveTo.Value.ToString("O") : "+∞";
        return $"[{from}, {to})";
    }
}
