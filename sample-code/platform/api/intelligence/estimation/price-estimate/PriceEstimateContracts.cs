namespace Whycespace.Platform.Api.Intelligence.Estimation.PriceEstimate;

public sealed record PriceEstimateRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PriceEstimateResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
