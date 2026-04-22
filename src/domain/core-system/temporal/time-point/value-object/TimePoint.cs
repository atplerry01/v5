namespace Whycespace.Domain.CoreSystem.Temporal.TimePoint;

public sealed record TimePoint : IComparable<TimePoint>
{
    public DateTimeOffset Timestamp { get; }
    public string? Label { get; }

    public TimePoint(DateTimeOffset timestamp, string? label = null)
    {
        if (timestamp == default)
            throw TimePointErrors.TimestampMustNotBeDefault();

        Timestamp = timestamp.ToUniversalTime();
        Label = label;
    }

    public bool IsBefore(TimePoint other) => Timestamp < other.Timestamp;
    public bool IsAfter(TimePoint other) => Timestamp > other.Timestamp;
    public bool IsSameInstant(TimePoint other) => Timestamp == other.Timestamp;

    public int CompareTo(TimePoint? other)
    {
        if (other is null) return 1;
        return Timestamp.CompareTo(other.Timestamp);
    }

    public static bool operator <(TimePoint left, TimePoint right) =>
        left.Timestamp < right.Timestamp;
    public static bool operator >(TimePoint left, TimePoint right) =>
        left.Timestamp > right.Timestamp;
    public static bool operator <=(TimePoint left, TimePoint right) =>
        left.Timestamp <= right.Timestamp;
    public static bool operator >=(TimePoint left, TimePoint right) =>
        left.Timestamp >= right.Timestamp;

    public TimeSpan DistanceTo(TimePoint other) =>
        (other.Timestamp - Timestamp).Duration();

    public override string ToString() =>
        Label is null ? Timestamp.ToString("O") : $"{Label}={Timestamp:O}";
}
