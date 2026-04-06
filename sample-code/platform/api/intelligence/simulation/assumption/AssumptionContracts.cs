namespace Whycespace.Platform.Api.Intelligence.Simulation.Assumption;

public sealed record AssumptionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AssumptionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
