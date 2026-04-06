namespace Whycespace.Platform.Api.Core.Command.CommandEnvelope;

public sealed record CommandEnvelopeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CommandEnvelopeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
