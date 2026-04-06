namespace Whycespace.Projections.Decision.Governance.Quorum;

public sealed record QuorumView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
