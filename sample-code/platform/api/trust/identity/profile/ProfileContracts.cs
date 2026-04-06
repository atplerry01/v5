namespace Whycespace.Platform.Api.Trust.Identity.Profile;

public sealed record ProfileRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ProfileResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
