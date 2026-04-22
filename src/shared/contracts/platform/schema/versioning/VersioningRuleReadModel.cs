namespace Whycespace.Shared.Contracts.Platform.Schema.Versioning;

public sealed record VersioningRuleReadModel
{
    public Guid VersioningRuleId { get; init; }
    public Guid SchemaRef { get; init; }
    public int FromVersion { get; init; }
    public int ToVersion { get; init; }
    public string EvolutionClass { get; init; } = string.Empty;
    public string? Verdict { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
