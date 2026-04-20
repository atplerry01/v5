namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record LifecycleStageChangedEvent(
    LifecycleId LifecycleId,
    LifecycleStage From,
    LifecycleStage To,
    DateTimeOffset ChangedAt);
