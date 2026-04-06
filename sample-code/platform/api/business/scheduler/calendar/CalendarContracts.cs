namespace Whycespace.Platform.Api.Business.Scheduler.Calendar;

public sealed record CalendarRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CalendarResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
