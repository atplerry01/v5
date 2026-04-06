namespace Whycespace.Platform.Api.Business.Billing.Adjustment;

public sealed record AdjustmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AdjustmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
