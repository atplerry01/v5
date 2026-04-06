namespace Whycespace.Platform.Api.Intelligence.Observability.Trace;

public sealed record TraceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TraceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
