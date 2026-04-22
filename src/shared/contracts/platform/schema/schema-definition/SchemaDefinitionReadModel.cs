namespace Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;

public sealed record SchemaDefinitionReadModel
{
    public Guid SchemaDefinitionId { get; init; }
    public string SchemaName { get; init; } = string.Empty;
    public int Version { get; init; }
    public string CompatibilityMode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
