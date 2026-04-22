using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Event.EventDefinition;

public sealed record DefineEventCommand(
    Guid EventDefinitionId,
    string TypeName,
    string Version,
    string SchemaId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    DateTimeOffset DefinedAt) : IHasAggregateId
{
    public Guid AggregateId => EventDefinitionId;
}

public sealed record DeprecateEventDefinitionCommand(
    Guid EventDefinitionId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => EventDefinitionId;
}
