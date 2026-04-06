namespace Whycespace.Platform.Api.Intelligence.Economic.Simulation;

public sealed record SimulationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record SimulationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
