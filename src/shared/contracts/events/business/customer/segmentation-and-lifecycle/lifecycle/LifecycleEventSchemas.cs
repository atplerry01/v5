namespace Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record LifecycleStartedEventSchema(
    Guid AggregateId,
    Guid CustomerId,
    string InitialStage,
    DateTimeOffset StartedAt);

public sealed record LifecycleStageChangedEventSchema(
    Guid AggregateId,
    string From,
    string To,
    DateTimeOffset ChangedAt);

public sealed record LifecycleClosedEventSchema(
    Guid AggregateId,
    DateTimeOffset ClosedAt);
