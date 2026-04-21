namespace Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;

public sealed record HierarchyDefinitionReadModel
{
    public Guid HierarchyDefinitionId { get; init; }
    public string HierarchyName { get; init; } = string.Empty;
    public Guid ParentReference { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
