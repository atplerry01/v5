namespace Whycespace.Projections.Decision.Audit.Finding;

public sealed record FindingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
