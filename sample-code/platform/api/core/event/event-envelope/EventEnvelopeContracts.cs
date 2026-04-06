namespace Whycespace.Platform.Api.Core.Event.EventEnvelope;

public sealed record EventEnvelopeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EventEnvelopeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
