using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;

public sealed record DefineTypeDefinitionCommand(
    Guid TypeDefinitionId,
    string TypeName,
    string TypeCategory) : IHasAggregateId
{
    public Guid AggregateId => TypeDefinitionId;
}

public sealed record ActivateTypeDefinitionCommand(
    Guid TypeDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => TypeDefinitionId;
}

public sealed record RetireTypeDefinitionCommand(
    Guid TypeDefinitionId) : IHasAggregateId
{
    public Guid AggregateId => TypeDefinitionId;
}
