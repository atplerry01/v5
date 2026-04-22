using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;

public sealed record DraftSchemaDefinitionCommand(
    Guid SchemaDefinitionId,
    string SchemaName,
    int Version,
    IReadOnlyList<FieldDescriptorDto> Fields,
    string CompatibilityMode,
    DateTimeOffset DraftedAt) : IHasAggregateId
{
    public Guid AggregateId => SchemaDefinitionId;
}

public sealed record PublishSchemaDefinitionCommand(
    Guid SchemaDefinitionId,
    DateTimeOffset PublishedAt) : IHasAggregateId
{
    public Guid AggregateId => SchemaDefinitionId;
}

public sealed record DeprecateSchemaDefinitionCommand(
    Guid SchemaDefinitionId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => SchemaDefinitionId;
}

public sealed record FieldDescriptorDto(
    string FieldName,
    string FieldType,
    bool Required,
    string? DefaultValue,
    string? Description);
