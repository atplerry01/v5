namespace Whycespace.Shared.Contracts.Platform.Command.CommandDefinition;

public sealed record CommandDefinitionReadModel
{
    public Guid CommandDefinitionId { get; init; }
    public string TypeName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string SchemaId { get; init; } = string.Empty;
    public string OwnerClassification { get; init; } = string.Empty;
    public string OwnerContext { get; init; } = string.Empty;
    public string OwnerDomain { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
