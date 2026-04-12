namespace Whycespace.Domain.BusinessSystem.Scheduler.Recurrence;

public readonly record struct RecurrencePattern
{
    public RecurrenceFrequency Frequency { get; }
    public int Interval { get; }
    public long StartTicks { get; }
    public long? EndTicks { get; }
    public int? MaxOccurrences { get; }

    public RecurrencePattern(RecurrenceFrequency frequency, int interval, long startTicks, long? endTicks, int? maxOccurrences)
    {
        if (!Enum.IsDefined(frequency))
            throw new ArgumentException("RecurrencePattern frequency must be a defined value.", nameof(frequency));
        if (interval <= 0)
            throw new ArgumentException("RecurrencePattern interval must be greater than zero.", nameof(interval));
        if (endTicks.HasValue && endTicks.Value <= startTicks)
            throw new ArgumentException("RecurrencePattern end must be after start.", nameof(endTicks));
        if (maxOccurrences.HasValue && maxOccurrences.Value <= 0)
            throw new ArgumentException("RecurrencePattern max occurrences must be greater than zero.", nameof(maxOccurrences));
        if (!endTicks.HasValue && !maxOccurrences.HasValue)
            throw new ArgumentException("RecurrencePattern must define bounds: either endTicks or maxOccurrences is required.");

        Frequency = frequency;
        Interval = interval;
        StartTicks = startTicks;
        EndTicks = endTicks;
        MaxOccurrences = maxOccurrences;
    }
}
