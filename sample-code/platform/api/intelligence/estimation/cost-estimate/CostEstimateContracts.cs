namespace Whycespace.Platform.Api.Intelligence.Estimation.CostEstimate;

public sealed record CostEstimateRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CostEstimateResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
