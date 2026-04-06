namespace Whycespace.Projections.Intelligence.Experiment.ResultAnalysis;

public sealed record ResultAnalysisView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
