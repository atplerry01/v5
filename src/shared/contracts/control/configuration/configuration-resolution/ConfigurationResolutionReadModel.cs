namespace Whycespace.Shared.Contracts.Control.Configuration.ConfigurationResolution;

public sealed record ConfigurationResolutionReadModel
{
    public Guid ResolutionId { get; init; }
    public string DefinitionId { get; init; } = string.Empty;
    public string ScopeId { get; init; } = string.Empty;
    public string StateId { get; init; } = string.Empty;
    public string ResolvedValue { get; init; } = string.Empty;
    public DateTimeOffset ResolvedAt { get; init; }
}
