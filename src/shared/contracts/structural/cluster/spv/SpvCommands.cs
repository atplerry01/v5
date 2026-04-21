using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Cluster.Spv;

public sealed record CreateSpvCommand(
    Guid SpvId,
    Guid ClusterReference,
    string SpvName,
    string SpvType) : IHasAggregateId
{
    public Guid AggregateId => SpvId;
}

public sealed record CreateSpvWithParentCommand(
    Guid SpvId,
    Guid ClusterReference,
    string SpvName,
    string SpvType,
    DateTimeOffset EffectiveAt) : IHasAggregateId
{
    public Guid AggregateId => SpvId;
}

public sealed record ActivateSpvCommand(
    Guid SpvId) : IHasAggregateId
{
    public Guid AggregateId => SpvId;
}

public sealed record SuspendSpvCommand(
    Guid SpvId) : IHasAggregateId
{
    public Guid AggregateId => SpvId;
}

public sealed record CloseSpvCommand(
    Guid SpvId) : IHasAggregateId
{
    public Guid AggregateId => SpvId;
}

public sealed record ReactivateSpvCommand(
    Guid SpvId) : IHasAggregateId
{
    public Guid AggregateId => SpvId;
}

public sealed record RetireSpvCommand(
    Guid SpvId) : IHasAggregateId
{
    public Guid AggregateId => SpvId;
}
