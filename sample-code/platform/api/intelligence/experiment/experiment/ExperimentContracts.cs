namespace Whycespace.Platform.Api.Intelligence.Experiment.Experiment;

public sealed record ExperimentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ExperimentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
