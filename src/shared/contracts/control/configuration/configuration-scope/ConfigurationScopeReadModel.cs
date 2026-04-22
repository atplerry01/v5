namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;

public sealed record ConfigurationScopeReadModel
{
    public Guid ScopeId { get; init; }
    public string DefinitionId { get; init; } = string.Empty;
    public string Classification { get; init; } = string.Empty;
    public string? Context { get; init; }
    public bool IsRemoved { get; init; }
}
