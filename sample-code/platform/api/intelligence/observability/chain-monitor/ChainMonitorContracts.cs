namespace Whycespace.Platform.Api.Intelligence.Observability.ChainMonitor;

public sealed record ChainMonitorRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ChainMonitorResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
