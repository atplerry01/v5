namespace Whycespace.Projections.Decision.Risk.Assessment;

public sealed record AssessmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
