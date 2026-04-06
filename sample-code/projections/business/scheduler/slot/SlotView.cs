namespace Whycespace.Projections.Business.Scheduler.Slot;

public sealed record SlotView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
