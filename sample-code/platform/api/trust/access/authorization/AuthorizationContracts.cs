namespace Whycespace.Platform.Api.Trust.Access.Authorization;

public sealed record AuthorizationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AuthorizationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
