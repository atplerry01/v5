namespace Whycespace.Platform.Api.Intelligence.Observability.Health;

public sealed record HealthRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record HealthResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
