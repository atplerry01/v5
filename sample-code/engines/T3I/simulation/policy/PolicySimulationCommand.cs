namespace Whycespace.Engines.T3I.PolicySimulation;

public sealed record PolicySimulationCommand
{
    public required Guid ScenarioId { get; init; }
    public required IReadOnlyList<PolicySimulationTarget> Targets { get; init; }
    public required SimulationContext Context { get; init; }
    public DateTimeOffset? SimulatedTime { get; init; }
    public bool IncludeImpactAnalysis { get; init; } = true;
    public bool IncludeRiskScoring { get; init; } = true;
    public bool IncludeAnomalyDetection { get; init; } = true;

    /// <summary>
    /// If set, restores a previously captured snapshot for reproducibility.
    /// Same snapshot_id + same seed → same output.
    /// </summary>
    public Guid? SnapshotId { get; init; }

    /// <summary>
    /// Deterministic seed for any stochastic simulation components.
    /// When provided, guarantees reproducible results.
    /// </summary>
    public int? Seed { get; init; }

    /// <summary>
    /// Number of simulation runs for stochastic aggregation.
    /// Default: 1 (single deterministic run).
    /// </summary>
    public int RunCount { get; init; } = 1;

    /// <summary>
    /// Include confidence scoring based on historical calibration.
    /// </summary>
    public bool IncludeConfidenceScoring { get; init; } = true;

    /// <summary>
    /// Include drift detection against historical outcomes.
    /// </summary>
    public bool IncludeDriftDetection { get; init; } = true;
}

public sealed record PolicySimulationTarget(
    Guid PolicyId,
    int? Version,
    bool IsOverride);

public sealed record SimulationContext(
    Guid ActorId,
    string Action,
    string Resource,
    string Environment,
    string? Classification);

public sealed record BatchSimulationCommand
{
    public required IReadOnlyList<PolicySimulationCommand> Scenarios { get; init; }
}
