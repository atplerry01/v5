namespace Whycespace.Platform.Api.Intelligence.Experiment.Cohort;

public sealed record CohortRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CohortResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
