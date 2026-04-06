namespace Whycespace.Platform.Api.Intelligence.Simulation.Outcome;

public sealed record OutcomeRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record OutcomeResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
