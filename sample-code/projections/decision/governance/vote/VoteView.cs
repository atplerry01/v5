namespace Whycespace.Projections.Decision.Governance.Vote;

public sealed record VoteView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
