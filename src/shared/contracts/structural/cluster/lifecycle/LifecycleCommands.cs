using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;

public sealed record DefineLifecycleCommand(
    Guid LifecycleId,
    Guid ClusterReference,
    string LifecycleName) : IHasAggregateId
{
    public Guid AggregateId => LifecycleId;
}

public sealed record TransitionLifecycleCommand(
    Guid LifecycleId) : IHasAggregateId
{
    public Guid AggregateId => LifecycleId;
}

public sealed record CompleteLifecycleCommand(
    Guid LifecycleId) : IHasAggregateId
{
    public Guid AggregateId => LifecycleId;
}
