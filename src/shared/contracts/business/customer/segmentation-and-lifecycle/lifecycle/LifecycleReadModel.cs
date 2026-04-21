namespace Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record LifecycleReadModel
{
    public Guid LifecycleId { get; init; }
    public Guid CustomerId { get; init; }
    public string Stage { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
