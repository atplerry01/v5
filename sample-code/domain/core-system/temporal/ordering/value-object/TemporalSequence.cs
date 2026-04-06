namespace Whycespace.Domain.CoreSystem.Temporal.Ordering;

/// <summary>
/// Tracks the last observed timestamp to enforce monotonic ordering.
/// </summary>
public sealed record TemporalSequence
{
    public long SequenceNumber { get; }
    public DateTimeOffset LastTimestamp { get; }

    private TemporalSequence(long sequenceNumber, DateTimeOffset lastTimestamp)
    {
        SequenceNumber = sequenceNumber;
        LastTimestamp = lastTimestamp;
    }

    public static TemporalSequence Initial(DateTimeOffset startTime) => new(0, startTime);

    public TemporalSequence Advance(DateTimeOffset timestamp) =>
        timestamp <= LastTimestamp
            ? throw new ArgumentException($"Timestamp {timestamp} is not after last timestamp {LastTimestamp}. Monotonic ordering violated.")
            : new(SequenceNumber + 1, timestamp);

    public bool IsMonotonicWith(DateTimeOffset next) => next > LastTimestamp;
}
