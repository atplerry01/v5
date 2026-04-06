namespace Whycespace.Projections.Intelligence.Planning.Plan;

public sealed record PlanView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
