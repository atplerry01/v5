namespace Whycespace.Projections.Business.Scheduler.Availability;

public sealed record AvailabilityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
