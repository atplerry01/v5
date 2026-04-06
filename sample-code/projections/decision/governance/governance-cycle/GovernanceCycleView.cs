namespace Whycespace.Projections.Decision.Governance.GovernanceCycle;

public sealed record GovernanceCycleView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
