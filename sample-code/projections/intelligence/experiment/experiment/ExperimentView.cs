namespace Whycespace.Projections.Intelligence.Experiment.Experiment;

public sealed record ExperimentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
