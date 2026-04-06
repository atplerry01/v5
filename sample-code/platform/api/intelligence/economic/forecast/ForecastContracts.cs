namespace Whycespace.Platform.Api.Intelligence.Economic.Forecast;

public sealed record ForecastRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ForecastResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
