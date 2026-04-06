namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyDefinition
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required ScopeDefinition Scope { get; init; }
    public required int Priority { get; init; }
    public required IReadOnlyList<RuleDefinition> Rules { get; init; }
}
