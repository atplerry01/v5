namespace Whycespace.Projections.Intelligence.Planning.CapacityPlan;

public sealed record CapacityPlanView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
