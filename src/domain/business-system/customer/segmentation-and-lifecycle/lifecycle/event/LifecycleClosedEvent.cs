namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record LifecycleClosedEvent(LifecycleId LifecycleId, DateTimeOffset ClosedAt);
