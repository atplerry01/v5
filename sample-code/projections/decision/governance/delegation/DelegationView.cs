namespace Whycespace.Projections.Decision.Governance.Delegation;

public sealed record DelegationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
