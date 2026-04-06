namespace Whycespace.Platform.Api.Trust.Identity.Registry;

public sealed record RegistryRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RegistryResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
