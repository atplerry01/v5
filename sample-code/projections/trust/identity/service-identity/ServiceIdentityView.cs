namespace Whycespace.Projections.Trust.Identity.ServiceIdentity;

public sealed record ServiceIdentityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
