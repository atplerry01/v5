namespace Whycespace.Projections.Trust.Identity.Profile;

public sealed record ProfileView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
