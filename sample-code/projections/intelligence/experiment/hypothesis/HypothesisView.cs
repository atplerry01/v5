namespace Whycespace.Projections.Intelligence.Experiment.Hypothesis;

public sealed record HypothesisView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
