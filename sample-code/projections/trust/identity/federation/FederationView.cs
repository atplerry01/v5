namespace Whycespace.Projections.Trust.Identity.Federation;

public sealed record FederationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
