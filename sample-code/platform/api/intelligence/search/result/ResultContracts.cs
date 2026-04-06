namespace Whycespace.Platform.Api.Intelligence.Search.Result;

public sealed record ResultRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ResultResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
