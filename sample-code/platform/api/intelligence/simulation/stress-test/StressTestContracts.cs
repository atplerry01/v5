namespace Whycespace.Platform.Api.Intelligence.Simulation.StressTest;

public sealed record StressTestRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record StressTestResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
