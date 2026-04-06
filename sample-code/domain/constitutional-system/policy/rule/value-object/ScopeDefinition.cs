namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record ScopeDefinition
{
    public required string Type { get; init; }
    public required string Target { get; init; }

    public static ScopeDefinition Global() => new() { Type = "global", Target = "*" };
}
