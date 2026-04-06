namespace Whycespace.Projections.Decision.Governance.Proposal;

public sealed record ProposalView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
