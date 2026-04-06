namespace Whycespace.Platform.Api.Intelligence.Index.CostIndex;

public sealed record CostIndexRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CostIndexResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
