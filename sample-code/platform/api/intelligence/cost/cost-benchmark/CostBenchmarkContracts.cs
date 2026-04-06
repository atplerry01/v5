namespace Whycespace.Platform.Api.Intelligence.Cost.CostBenchmark;

public sealed record CostBenchmarkRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CostBenchmarkResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
