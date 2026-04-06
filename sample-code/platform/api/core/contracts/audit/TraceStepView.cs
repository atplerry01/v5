namespace Whycespace.Platform.Api.Core.Contracts.Audit;

/// <summary>
/// Read-only trace step in the execution trace.
/// Represents a single step within the traced workflow execution.
/// Sourced from pre-built trace projections — no event replay.
/// </summary>
public sealed record TraceStepView
{
    public required string StepName { get; init; }
    public required string Layer { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public TimeSpan? Duration { get; init; }
    public string? Error { get; init; }
}
