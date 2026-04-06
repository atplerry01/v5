namespace Whycespace.Projections.Decision.Risk.Mitigation;

public sealed record MitigationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
