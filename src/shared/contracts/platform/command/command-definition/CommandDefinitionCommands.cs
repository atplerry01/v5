using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;

public sealed record DefineCommandCommand(
    Guid CommandDefinitionId,
    string TypeName,
    int Version,
    string SchemaId,
    string OwnerClassification,
    string OwnerContext,
    string OwnerDomain,
    DateTimeOffset DefinedAt) : IHasAggregateId
{
    public Guid AggregateId => CommandDefinitionId;
}

public sealed record DeprecateCommandDefinitionCommand(
    Guid CommandDefinitionId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => CommandDefinitionId;
}
