namespace Whycespace.Projections.Business.Scheduler.Booking;

public sealed record BookingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
