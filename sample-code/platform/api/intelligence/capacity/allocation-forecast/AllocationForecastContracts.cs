namespace Whycespace.Platform.Api.Intelligence.Capacity.AllocationForecast;

public sealed record AllocationForecastRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AllocationForecastResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
