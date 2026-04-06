namespace Whycespace.Platform.Api.Intelligence.Estimation.Benchmark;

public sealed record BenchmarkRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record BenchmarkResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
