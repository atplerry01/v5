namespace Whycespace.Platform.Api.Core.Temporal.ScheduleReference;

public sealed record ScheduleReferenceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ScheduleReferenceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
