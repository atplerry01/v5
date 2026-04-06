namespace Whycespace.Platform.Api.Intelligence.Simulation.Recommendation;

public sealed record RecommendationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RecommendationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
