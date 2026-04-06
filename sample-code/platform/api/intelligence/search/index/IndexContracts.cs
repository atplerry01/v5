namespace Whycespace.Platform.Api.Intelligence.Search.Index;

public sealed record IndexRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record IndexResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
