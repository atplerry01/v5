namespace Whycespace.Platform.Api.Core.Contracts.Simulation;

/// <summary>
/// Read-only simulation result returned to the caller.
/// Contains decision outcome, warnings, violations, and projected impact.
/// Fully deterministic for the same input — no side effects.
///
/// Decision values: SUCCESS / FAIL / WARNING
/// </summary>
public sealed record SimulationResultView
{
    public required string Decision { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
    public required IReadOnlyList<string> Violations { get; init; }
    public required SimulationImpactView Impact { get; init; }
    public PolicyPreview? PolicyPreview { get; init; }
}