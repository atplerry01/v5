using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record LifecycleStartedEvent(
    LifecycleId LifecycleId,
    CustomerRef Customer,
    LifecycleStage InitialStage,
    DateTimeOffset StartedAt);
