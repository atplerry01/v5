using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;

public sealed record CreateTopologyDefinitionCommand(
    Guid TopologyDefinitionId,
    string DefinitionName,
    string DefinitionKind) : IHasAggregateId
{
    public Guid AggregateId => TopologyDefinitionId;
}

public sealed record ActivateTopologyDefinitionCommand(
    Guid TopologyDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => TopologyDefinitionId;
}

public sealed record SuspendTopologyDefinitionCommand(
    Guid TopologyDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => TopologyDefinitionId;
}

public sealed record ReactivateTopologyDefinitionCommand(
    Guid TopologyDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => TopologyDefinitionId;
}

public sealed record RetireTopologyDefinitionCommand(
    Guid TopologyDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => TopologyDefinitionId;
}
