namespace Whycespace.Platform.Api.Intelligence.Experiment.Hypothesis;

public sealed record HypothesisRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record HypothesisResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
