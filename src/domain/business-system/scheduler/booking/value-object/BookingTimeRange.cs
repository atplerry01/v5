namespace Whycespace.Domain.BusinessSystem.Scheduler.Booking;

public readonly record struct BookingTimeRange
{
    public long StartTicks { get; }
    public long EndTicks { get; }

    public BookingTimeRange(long startTicks, long endTicks)
    {
        if (endTicks <= startTicks)
            throw new ArgumentException("BookingTimeRange end must be after start.", nameof(endTicks));
        StartTicks = startTicks;
        EndTicks = endTicks;
    }

    public bool OverlapsWith(BookingTimeRange other)
    {
        return StartTicks < other.EndTicks && other.StartTicks < EndTicks;
    }
}
