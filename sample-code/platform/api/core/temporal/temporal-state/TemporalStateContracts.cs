namespace Whycespace.Platform.Api.Core.Temporal.TemporalState;

public sealed record TemporalStateRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TemporalStateResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
