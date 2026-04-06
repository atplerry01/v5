namespace Whycespace.Platform.Api.Intelligence.Simulation.Scenario;

public sealed record ScenarioRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ScenarioResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
