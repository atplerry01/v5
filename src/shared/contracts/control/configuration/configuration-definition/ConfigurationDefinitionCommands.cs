using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;

public sealed record DefineConfigurationCommand(
    Guid DefinitionId,
    string Name,
    string ValueType,
    string Description,
    string? DefaultValue = null) : IHasAggregateId
{
    public Guid AggregateId => DefinitionId;
}

public sealed record DeprecateConfigurationDefinitionCommand(
    Guid DefinitionId) : IHasAggregateId
{
    public Guid AggregateId => DefinitionId;
}
