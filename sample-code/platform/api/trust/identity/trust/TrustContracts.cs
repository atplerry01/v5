namespace Whycespace.Platform.Api.Trust.Identity.Trust;

public sealed record TrustRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TrustResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
