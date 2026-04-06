namespace Whycespace.Projections.Trust.Identity.IdentityGraph;

public sealed record IdentityGraphView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
