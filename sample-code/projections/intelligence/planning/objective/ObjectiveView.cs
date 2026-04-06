namespace Whycespace.Projections.Intelligence.Planning.Objective;

public sealed record ObjectiveView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
