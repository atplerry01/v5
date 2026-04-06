namespace Whycespace.Platform.Api.Intelligence.Search.Query;

public sealed record QueryRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record QueryResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
