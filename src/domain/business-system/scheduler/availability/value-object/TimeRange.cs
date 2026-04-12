namespace Whycespace.Domain.BusinessSystem.Scheduler.Availability;

public readonly record struct TimeRange
{
    public long StartTicks { get; }
    public long EndTicks { get; }

    public TimeRange(long startTicks, long endTicks)
    {
        if (endTicks <= startTicks)
            throw new ArgumentException("TimeRange end must be after start.", nameof(endTicks));
        StartTicks = startTicks;
        EndTicks = endTicks;
    }
}
