namespace Whycespace.Platform.Api.Intelligence.Experiment.ResultAnalysis;

public sealed record ResultAnalysisRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ResultAnalysisResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
