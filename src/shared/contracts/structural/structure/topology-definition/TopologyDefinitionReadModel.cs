namespace Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;

public sealed record TopologyDefinitionReadModel
{
    public Guid TopologyDefinitionId { get; init; }
    public string DefinitionName { get; init; } = string.Empty;
    public string DefinitionKind { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
