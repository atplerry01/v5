namespace Whycespace.Platform.Api.Decision.Risk.Threshold;

public sealed record ThresholdRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ThresholdResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
