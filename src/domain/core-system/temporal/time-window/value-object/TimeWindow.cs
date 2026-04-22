using Whycespace.Domain.CoreSystem.Temporal.TimeRange;

namespace Whycespace.Domain.CoreSystem.Temporal.TimeWindow;

// Half-open interval [Start, End) — start inclusive, end exclusive.
public sealed record TimeWindow
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset? End { get; }

    public TimeWindow(DateTimeOffset start, DateTimeOffset? end = null)
    {
        if (start == default)
            throw TimeWindowErrors.StartMustNotBeDefault();

        var utcStart = start.ToUniversalTime();
        var utcEnd = end?.ToUniversalTime();

        if (utcEnd.HasValue && utcEnd.Value <= utcStart)
            throw TimeWindowErrors.EndMustFollowStart(utcStart, utcEnd.Value);

        Start = utcStart;
        End = utcEnd;
    }

    public bool IsOpen => !End.HasValue;

    public TimeSpan? Duration => End.HasValue ? End.Value - Start : null;

    // [Start, End) — start inclusive, end exclusive.
    public bool Contains(DateTimeOffset point)
    {
        var utc = point.ToUniversalTime();
        return utc >= Start && (!End.HasValue || utc < End.Value);
    }

    public bool Overlaps(TimeWindow other) =>
        Start < (other.End ?? DateTimeOffset.MaxValue) &&
        (End ?? DateTimeOffset.MaxValue) > other.Start;

    // Only valid when the window is closed; throws otherwise.
    public TimeRange.TimeRange ToTimeRange()
    {
        if (!End.HasValue)
            throw TimeWindowErrors.CannotConvertOpenWindowToTimeRange();

        return new TimeRange.TimeRange(Start, End.Value);
    }

    public override string ToString() =>
        End.HasValue ? $"[{Start:O}, {End.Value:O})" : $"[{Start:O}, ∞)";
}
