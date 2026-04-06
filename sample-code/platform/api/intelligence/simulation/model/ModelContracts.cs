namespace Whycespace.Platform.Api.Intelligence.Simulation.Model;

public sealed record ModelRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ModelResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
