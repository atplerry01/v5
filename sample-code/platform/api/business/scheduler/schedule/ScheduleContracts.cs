namespace Whycespace.Platform.Api.Business.Scheduler.Schedule;

public sealed record ScheduleRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ScheduleResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
