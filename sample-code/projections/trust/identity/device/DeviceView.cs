namespace Whycespace.Projections.Trust.Identity.Device;

public sealed record DeviceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
