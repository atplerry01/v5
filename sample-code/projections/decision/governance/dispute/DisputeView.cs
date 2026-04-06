namespace Whycespace.Projections.Decision.Governance.Dispute;

public sealed record DisputeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
