namespace Whycespace.Projections.Constitutional.Policy.Constraint;

public sealed record ConstraintView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
