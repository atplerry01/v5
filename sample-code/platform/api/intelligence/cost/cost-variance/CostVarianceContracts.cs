namespace Whycespace.Platform.Api.Intelligence.Cost.CostVariance;

public sealed record CostVarianceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CostVarianceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
