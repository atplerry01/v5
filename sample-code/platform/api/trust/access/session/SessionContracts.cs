namespace Whycespace.Platform.Api.Trust.Access.Session;

public sealed record SessionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SessionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
