namespace Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;

public sealed record TypeDefinitionReadModel
{
    public Guid TypeDefinitionId { get; init; }
    public string TypeName { get; init; } = string.Empty;
    public string TypeCategory { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
