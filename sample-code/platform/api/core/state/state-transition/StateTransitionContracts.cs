namespace Whycespace.Platform.Api.Core.State.StateTransition;

public sealed record StateTransitionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StateTransitionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
