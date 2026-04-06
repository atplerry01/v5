namespace Whycespace.Platform.Api.Business.Portfolio.Performance;

public sealed record PerformanceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PerformanceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
