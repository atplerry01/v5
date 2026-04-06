namespace Whycespace.Platform.Api.Intelligence.Economic.Optimization;

public sealed record OptimizationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record OptimizationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
