namespace Whycespace.Platform.Api.Intelligence.Index.PerformanceIndex;

public sealed record PerformanceIndexRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PerformanceIndexResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
