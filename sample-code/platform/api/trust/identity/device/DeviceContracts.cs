namespace Whycespace.Platform.Api.Trust.Identity.Device;

public sealed record DeviceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DeviceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
