namespace Whycespace.Platform.Api.Intelligence.Economic._shared;

public sealed record _sharedRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record _sharedResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
