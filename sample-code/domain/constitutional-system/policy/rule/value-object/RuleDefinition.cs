namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record RuleDefinition
{
    public required string Name { get; init; }
    public required string Decision { get; init; }
    public required IReadOnlyList<ConstraintDefinition> Constraints { get; init; }
}
