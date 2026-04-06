namespace Whycespace.Projections.Intelligence.Planning.ScenarioPlan;

public sealed record ScenarioPlanView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
