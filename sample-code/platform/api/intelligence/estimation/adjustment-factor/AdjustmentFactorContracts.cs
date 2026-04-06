namespace Whycespace.Platform.Api.Intelligence.Estimation.AdjustmentFactor;

public sealed record AdjustmentFactorRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AdjustmentFactorResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
