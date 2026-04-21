using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;

public sealed record DefineHierarchyDefinitionCommand(
    Guid HierarchyDefinitionId,
    string HierarchyName,
    Guid ParentReference) : IHasAggregateId
{
    public Guid AggregateId => HierarchyDefinitionId;
}

public sealed record ValidateHierarchyDefinitionCommand(
    Guid HierarchyDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => HierarchyDefinitionId;
}

public sealed record LockHierarchyDefinitionCommand(
    Guid HierarchyDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => HierarchyDefinitionId;
}
