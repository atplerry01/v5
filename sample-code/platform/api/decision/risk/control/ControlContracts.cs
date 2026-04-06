namespace Whycespace.Platform.Api.Decision.Risk.Control;

public sealed record ControlRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ControlResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
