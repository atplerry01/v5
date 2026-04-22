namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDefinition;

public sealed record PolicyDefinitionReadModel
{
    public Guid PolicyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ScopeClassification { get; init; } = string.Empty;
    public string? ScopeContext { get; init; }
    public string ScopeActionMask { get; init; } = string.Empty;
    public int Version { get; init; }
    public string Status { get; init; } = string.Empty;
}
