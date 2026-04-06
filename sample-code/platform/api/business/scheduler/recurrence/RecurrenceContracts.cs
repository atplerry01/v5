namespace Whycespace.Platform.Api.Business.Scheduler.Recurrence;

public sealed record RecurrenceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RecurrenceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
