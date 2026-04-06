namespace Whycespace.Platform.Api.Core.Event.EventStream;

public sealed record EventStreamRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EventStreamResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
