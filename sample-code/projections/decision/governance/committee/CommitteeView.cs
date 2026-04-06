namespace Whycespace.Projections.Decision.Governance.Committee;

public sealed record CommitteeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
