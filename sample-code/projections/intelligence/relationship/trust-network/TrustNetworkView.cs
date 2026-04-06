namespace Whycespace.Projections.Intelligence.Relationship.TrustNetwork;

public sealed record TrustNetworkView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
