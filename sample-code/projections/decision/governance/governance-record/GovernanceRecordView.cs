namespace Whycespace.Projections.Decision.Governance.GovernanceRecord;

public sealed record GovernanceRecordView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
