namespace Whycespace.Platform.Api.Intelligence.Observability.Metric;

public sealed record MetricRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record MetricResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
