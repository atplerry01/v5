namespace Whycespace.Domain.BusinessSystem.Scheduler.Slot;

public readonly record struct TimeSlot
{
    public long StartTicks { get; }
    public long EndTicks { get; }

    public TimeSlot(long startTicks, long endTicks)
    {
        if (endTicks <= startTicks)
            throw new ArgumentException("TimeSlot end must be after start (duration must be > 0).", nameof(endTicks));
        StartTicks = startTicks;
        EndTicks = endTicks;
    }
}
