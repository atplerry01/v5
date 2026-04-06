namespace Whycespace.Platform.Api.Core.Temporal.Clock;

public sealed record ClockRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ClockResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
