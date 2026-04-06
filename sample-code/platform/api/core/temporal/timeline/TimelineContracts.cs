namespace Whycespace.Platform.Api.Core.Temporal.Timeline;

public sealed record TimelineRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TimelineResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
