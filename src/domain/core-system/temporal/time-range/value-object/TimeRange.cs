namespace Whycespace.Domain.CoreSystem.Temporal.TimeRange;

// Closed interval [Start, End] — both boundaries inclusive.
public sealed record TimeRange
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public TimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (start == default)
            throw TimeRangeErrors.StartMustNotBeDefault();

        if (end == default)
            throw TimeRangeErrors.EndMustNotBeDefault();

        var utcStart = start.ToUniversalTime();
        var utcEnd = end.ToUniversalTime();

        if (utcEnd <= utcStart)
            throw TimeRangeErrors.EndMustFollowStart(utcStart, utcEnd);

        Start = utcStart;
        End = utcEnd;
    }

    public TimeSpan Duration => End - Start;
    public DateTimeOffset Midpoint => Start + TimeSpan.FromTicks(Duration.Ticks / 2);

    // [Start, End] — both inclusive.
    public bool Contains(DateTimeOffset point)
    {
        var utc = point.ToUniversalTime();
        return utc >= Start && utc <= End;
    }

    public bool Overlaps(TimeRange other) => Start < other.End && End > other.Start;
    public bool Precedes(TimeRange other) => End <= other.Start;
    public bool Follows(TimeRange other) => Start >= other.End;
    public bool IsAdjacent(TimeRange other) => End == other.Start || other.End == Start;

    // Returns the gap between this range and another non-overlapping range, or null if they overlap/touch.
    public TimeSpan? Gap(TimeRange other)
    {
        if (Overlaps(other) || IsAdjacent(other)) return null;
        return Precedes(other) ? other.Start - End : Start - other.End;
    }

    public override string ToString() => $"[{Start:O}, {End:O}]";
}
