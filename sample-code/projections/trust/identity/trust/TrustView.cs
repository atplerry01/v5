namespace Whycespace.Projections.Trust.Identity.Trust;

public sealed record TrustView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
