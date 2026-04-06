namespace Whycespace.Projections.Identity;

public sealed record IdentityDeviceReadModel
{
    public required string DeviceId { get; init; }
    public required string IdentityId { get; init; }
    public required string DeviceType { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }
    public DateTimeOffset? VerifiedAt { get; init; }
    public DateTimeOffset LastSeenAt { get; init; }
}
