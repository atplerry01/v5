namespace Whycespace.Shared.Contracts.Platform.Event.EventDefinition;

public sealed record EventDefinitionReadModel
{
    public Guid EventDefinitionId { get; init; }
    public string TypeName { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string SchemaId { get; init; } = string.Empty;
    public string SourceClassification { get; init; } = string.Empty;
    public string SourceContext { get; init; } = string.Empty;
    public string SourceDomain { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
