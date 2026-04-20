namespace Whycespace.Domain.BusinessSystem.Shared.Time;

// Shared time-window primitive for business-system leaves.
// - StartsAt is mandatory.
// - EndsAt is optional; when omitted the window is open-ended.
// - Consumers that require a closed window enforce EndsAt.HasValue at the
//   aggregate invariant level.
public readonly record struct TimeWindow
{
    public DateTimeOffset StartsAt { get; }
    public DateTimeOffset? EndsAt { get; }

    public TimeWindow(DateTimeOffset startsAt, DateTimeOffset? endsAt)
    {
        if (endsAt.HasValue && endsAt.Value <= startsAt)
            throw new ArgumentException("TimeWindow.EndsAt must be after StartsAt.", nameof(endsAt));

        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public bool IsClosed => EndsAt.HasValue;

    public bool Contains(DateTimeOffset at)
    {
        if (at < StartsAt) return false;
        if (EndsAt.HasValue && at >= EndsAt.Value) return false;
        return true;
    }

    public bool IsExpiredAt(DateTimeOffset at)
        => EndsAt.HasValue && at >= EndsAt.Value;
}
