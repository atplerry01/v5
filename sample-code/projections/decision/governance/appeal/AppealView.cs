namespace Whycespace.Projections.Decision.Governance.Appeal;

public sealed record AppealView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
