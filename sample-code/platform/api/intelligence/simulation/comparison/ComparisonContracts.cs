namespace Whycespace.Platform.Api.Intelligence.Simulation.Comparison;

public sealed record ComparisonRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ComparisonResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
