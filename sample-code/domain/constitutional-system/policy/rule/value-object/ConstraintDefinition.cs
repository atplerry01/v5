namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record ConstraintDefinition
{
    public required string Left { get; init; }
    public required string Operator { get; init; }
    public required string Right { get; init; }

    public bool IsFactReference => Left.Contains('.', StringComparison.Ordinal);
}
