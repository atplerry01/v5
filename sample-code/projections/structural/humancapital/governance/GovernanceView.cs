namespace Whycespace.Projections.Structural.Humancapital.Governance;

public sealed record GovernanceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
