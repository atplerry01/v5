namespace Whycespace.Projections.Decision.Governance.Sanction;

public sealed record SanctionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
