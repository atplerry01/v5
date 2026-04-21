using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed record StartLifecycleCommand(
    Guid LifecycleId,
    Guid CustomerId,
    string InitialStage,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => LifecycleId;
}

public sealed record ChangeLifecycleStageCommand(
    Guid LifecycleId,
    string To,
    DateTimeOffset ChangedAt) : IHasAggregateId
{
    public Guid AggregateId => LifecycleId;
}

public sealed record CloseLifecycleCommand(
    Guid LifecycleId,
    DateTimeOffset ClosedAt) : IHasAggregateId
{
    public Guid AggregateId => LifecycleId;
}
