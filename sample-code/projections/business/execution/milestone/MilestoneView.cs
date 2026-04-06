namespace Whycespace.Projections.Business.Execution.Milestone;

public sealed record MilestoneView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
