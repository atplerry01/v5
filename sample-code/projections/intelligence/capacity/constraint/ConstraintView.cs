namespace Whycespace.Projections.Intelligence.Capacity.Constraint;

public sealed record ConstraintView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
