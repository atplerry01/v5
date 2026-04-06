namespace Whycespace.Platform.Api.Trust.Access.Request;

public sealed record RequestRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RequestResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
