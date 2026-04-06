namespace Whycespace.Projections.Intelligence.Experiment.Cohort;

public sealed record CohortView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
