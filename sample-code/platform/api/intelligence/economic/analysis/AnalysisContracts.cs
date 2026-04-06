namespace Whycespace.Platform.Api.Intelligence.Economic.Analysis;

public sealed record AnalysisRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AnalysisResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
