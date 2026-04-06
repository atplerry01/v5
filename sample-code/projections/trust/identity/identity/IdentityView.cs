namespace Whycespace.Projections.Trust.Identity.Identity;

public sealed record IdentityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
