namespace Whycespace.Projections.Identity;

public sealed record IdentitySessionReadModel
{
    public required string SessionId { get; init; }
    public required string IdentityId { get; init; }
    public required string DeviceId { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset LastAccessedAt { get; init; }
}
