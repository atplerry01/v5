namespace Whycespace.Projections.Intelligence.Economic.Analysis;

public sealed record AnalysisView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
