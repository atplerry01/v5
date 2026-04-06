namespace Whycespace.Projections.Intelligence.Planning.SchedulePlan;

public sealed record SchedulePlanView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
