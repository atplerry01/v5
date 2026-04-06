namespace Whycespace.Projections.Decision.Governance.Guardian;

public sealed record GuardianView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
