namespace Whycespace.Platform.Api.Intelligence.Economic.Anomaly;

public sealed record AnomalyRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AnomalyResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
