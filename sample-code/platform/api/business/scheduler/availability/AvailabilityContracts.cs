namespace Whycespace.Platform.Api.Business.Scheduler.Availability;

public sealed record AvailabilityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AvailabilityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
