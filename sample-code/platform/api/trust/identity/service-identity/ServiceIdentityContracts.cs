namespace Whycespace.Platform.Api.Trust.Identity.ServiceIdentity;

public sealed record ServiceIdentityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ServiceIdentityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
