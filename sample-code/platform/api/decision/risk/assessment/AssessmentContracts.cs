namespace Whycespace.Platform.Api.Decision.Risk.Assessment;

public sealed record AssessmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AssessmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
