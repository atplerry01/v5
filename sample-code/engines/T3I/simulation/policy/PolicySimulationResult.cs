using Whycespace.Engines.T3I.Simulation.Policy;

namespace Whycespace.Engines.T3I.PolicySimulation;

public sealed record PolicySimulationResult
{
    public required Guid ScenarioId { get; init; }
    public required IReadOnlyList<SimulatedPolicyVersion> PolicyVersionsUsed { get; init; }
    public required DecisionSummary DecisionSummary { get; init; }
    public ImpactSummary? ImpactSummary { get; init; }
    public RiskScore? RiskScore { get; init; }
    public IReadOnlyList<SimulationAnomaly> Anomalies { get; init; } = [];
    public string? Recommendation { get; init; }
    public DateTimeOffset SimulatedAt { get; init; }
    public TimeSpan Duration { get; init; }

    // --- E12.1 Hardening Extensions (additive, non-breaking) ---

    /// <summary>Snapshot identifier for reproducibility. Same snapshot + seed → same result.</summary>
    public Guid? SnapshotId { get; init; }

    /// <summary>Confidence assessment based on historical calibration.</summary>
    public ConfidenceAssessment? Confidence { get; init; }

    /// <summary>Drift detection result comparing predicted vs actual outcomes.</summary>
    public DriftAssessment? Drift { get; init; }

    /// <summary>Aggregated statistics when multi-run simulation is used.</summary>
    public AggregatedSimulationResult? Aggregation { get; init; }

    /// <summary>Normalized impact scores for cross-domain comparison.</summary>
    public NormalizedImpactScores? NormalizedImpact { get; init; }

    // --- Federation Simulation Extensions (additive, non-breaking) ---

    /// <summary>Federation graph hash when simulation includes cross-cluster policies.</summary>
    public string? FederationGraphHash { get; init; }

    /// <summary>Cross-cluster conflicts detected during federation simulation.</summary>
    public IReadOnlyList<string>? CrossClusterConflicts { get; init; }

    /// <summary>Dependency chain across federated clusters.</summary>
    public IReadOnlyList<string>? FederationDependencyChain { get; init; }
}

public sealed record SimulatedPolicyVersion(
    Guid PolicyId,
    int Version,
    string Status,
    string? ArtifactHash);

public sealed record DecisionSummary(
    string OverallDecision,
    int PoliciesEvaluated,
    int RulesEvaluated,
    int RulesPassed,
    int RulesFailed,
    IReadOnlyList<string> Violations,
    IReadOnlyList<SimulatedDecision> PerPolicyDecisions);

public sealed record SimulatedDecision(
    Guid PolicyId,
    string Decision,
    IReadOnlyList<string> Violations);

public sealed record ImpactSummary(
    EconomicImpact Economic,
    OperationalImpact Operational,
    GovernanceImpact Governance);

public sealed record EconomicImpact(
    string Severity,
    double EstimatedRevenueEffect,
    double EstimatedCostEffect,
    string Explanation);

public sealed record OperationalImpact(
    string Severity,
    int AffectedWorkflows,
    int BlockedOperations,
    string Explanation);

public sealed record GovernanceImpact(
    string Severity,
    int AdditionalApprovalsRequired,
    int PolicyConflicts,
    string Explanation);

public sealed record RiskScore(
    int Score,
    string Category,
    IReadOnlyList<RiskFactor> Factors);

public sealed record RiskFactor(
    string Name,
    int Weight,
    string Description);

public sealed record SimulationAnomaly(
    string Type,
    string Severity,
    string Description,
    string? Recommendation);

public sealed record BatchSimulationResult(
    IReadOnlyList<PolicySimulationResult> Results,
    IReadOnlyList<PolicyConflictInteraction> Conflicts);

public sealed record PolicyConflictInteraction(
    Guid PolicyAId,
    Guid PolicyBId,
    string ConflictType,
    string Description);

// --- E12.1 Hardening Types ---

public sealed record AggregatedSimulationResult(
    int RunCount,
    double MeanRulesPassed,
    double MeanRulesFailed,
    double Variance,
    double ConfidenceIntervalLow,
    double ConfidenceIntervalHigh,
    string MostFrequentDecision);

public sealed record NormalizedImpactScores(
    double EconomicScore,
    double OperationalScore,
    double GovernanceScore,
    double CompositeScore);
