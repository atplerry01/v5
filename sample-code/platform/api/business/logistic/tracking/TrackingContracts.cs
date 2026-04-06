namespace Whycespace.Platform.Api.Business.Logistic.Tracking;

public sealed record TrackingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TrackingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
