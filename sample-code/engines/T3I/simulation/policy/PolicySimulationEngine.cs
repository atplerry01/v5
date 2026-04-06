using System.Diagnostics;
using Whycespace.Engines.T3I.PolicySimulation.Aggregator;
using Whycespace.Engines.T3I.PolicySimulation.Anomaly;
using Whycespace.Engines.T3I.Simulation.Policy;
using Whycespace.Engines.T3I.PolicySimulation.Evaluator;
using Whycespace.Engines.T3I.PolicySimulation.Forecast;
using Whycespace.Engines.T3I.PolicySimulation.Metrics;
using Whycespace.Engines.T3I.PolicySimulation.Risk;
using Whycespace.Engines.T3I.PolicySimulation.Scenario;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.PolicySimulation;

/// <summary>
/// T3I Policy Simulation &amp; Impact Engine.
/// Orchestrates: Snapshot → Scenario → Evaluation → Impact → Risk → Anomaly → Confidence → Drift → Metrics.
/// Read-only guarantee — NO writes to Postgres, WhyceChain, or active policy state.
/// Stateless execution — all state passed via command/result records.
/// </summary>
public sealed class PolicySimulationEngine
{
    private readonly PolicyScenarioBuilder _scenarioBuilder;
    private readonly PolicySimulationEvaluator _evaluator;
    private readonly PolicyImpactAnalyzer _impactAnalyzer;
    private readonly PolicyRiskEngine _riskEngine;
    private readonly PolicyAnomalyDetector _anomalyDetector;
    private readonly SimulationSnapshotProvider _snapshotProvider;
    private readonly PolicyConfidenceScorer _confidenceScorer;
    private readonly PolicyDriftDetector _driftDetector;
    private readonly PolicyCalibrationEngine _calibrationEngine;
    private readonly SimulationQualityMetrics _qualityMetrics;
    private readonly IClock _clock;

    public PolicySimulationEngine(
        PolicyScenarioBuilder scenarioBuilder,
        PolicySimulationEvaluator evaluator,
        PolicyImpactAnalyzer impactAnalyzer,
        PolicyRiskEngine riskEngine,
        PolicyAnomalyDetector anomalyDetector,
        SimulationSnapshotProvider snapshotProvider,
        PolicyConfidenceScorer confidenceScorer,
        PolicyDriftDetector driftDetector,
        PolicyCalibrationEngine calibrationEngine,
        SimulationQualityMetrics qualityMetrics,
        IClock clock)
    {
        _scenarioBuilder = scenarioBuilder ?? throw new ArgumentNullException(nameof(scenarioBuilder));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _impactAnalyzer = impactAnalyzer ?? throw new ArgumentNullException(nameof(impactAnalyzer));
        _riskEngine = riskEngine ?? throw new ArgumentNullException(nameof(riskEngine));
        _anomalyDetector = anomalyDetector ?? throw new ArgumentNullException(nameof(anomalyDetector));
        _snapshotProvider = snapshotProvider ?? throw new ArgumentNullException(nameof(snapshotProvider));
        _confidenceScorer = confidenceScorer ?? throw new ArgumentNullException(nameof(confidenceScorer));
        _driftDetector = driftDetector ?? throw new ArgumentNullException(nameof(driftDetector));
        _calibrationEngine = calibrationEngine ?? throw new ArgumentNullException(nameof(calibrationEngine));
        _qualityMetrics = qualityMetrics ?? throw new ArgumentNullException(nameof(qualityMetrics));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<PolicySimulationResult> SimulateAsync(
        PolicySimulationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Multi-run aggregation: delegate to aggregator if RunCount > 1
        if (command.RunCount > 1)
            return await SimulateWithAggregationAsync(command, cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        // 1. Snapshot capture or restore (for reproducibility)
        var referenceTime = command.SimulatedTime ?? _clock.UtcNowOffset;
        SimulationSnapshot snapshot;

        if (command.SnapshotId.HasValue)
        {
            snapshot = await _snapshotProvider.RestoreAsync(
                command.SnapshotId.Value, command.Targets, referenceTime, cancellationToken);
        }
        else
        {
            snapshot = await _snapshotProvider.CaptureAsync(
                command.Targets, referenceTime, cancellationToken);
        }

        // 2. Build scenario from snapshot
        var scenario = await _scenarioBuilder.BuildAsync(command, cancellationToken);

        // 3. Evaluate policies in simulation mode (no enforcement)
        var decisions = await _evaluator.EvaluateAsync(scenario, cancellationToken);

        // 4. Impact analysis (optional)
        ImpactSummary? impact = null;
        if (command.IncludeImpactAnalysis)
            impact = _impactAnalyzer.Analyze(scenario, decisions);

        // 5. Risk scoring (optional)
        RiskScore? risk = null;
        if (command.IncludeRiskScoring)
            risk = _riskEngine.Evaluate(scenario, decisions, impact);

        // 6. Anomaly detection (optional)
        IReadOnlyList<SimulationAnomaly> anomalies = [];
        if (command.IncludeAnomalyDetection)
            anomalies = _anomalyDetector.Detect(scenario, decisions, impact);

        // 7. Generate recommendation
        var recommendation = GenerateRecommendation(decisions, risk, anomalies);

        // 8. Impact normalization
        NormalizedImpactScores? normalizedImpact = null;
        if (impact is not null)
            normalizedImpact = ImpactNormalizer.Normalize(impact);

        stopwatch.Stop();

        // Build base result
        var result = new PolicySimulationResult
        {
            ScenarioId = command.ScenarioId,
            PolicyVersionsUsed = scenario.Policies
                .Where(p => p.Version is not null)
                .Select(p => new SimulatedPolicyVersion(
                    p.PolicyId, p.Version!.Version, p.Version.Status, p.Version.ArtifactHash))
                .ToList()
                .AsReadOnly(),
            DecisionSummary = decisions,
            ImpactSummary = impact,
            RiskScore = risk,
            Anomalies = anomalies,
            Recommendation = recommendation,
            SimulatedAt = scenario.SimulatedTime,
            Duration = stopwatch.Elapsed,
            SnapshotId = snapshot.SnapshotId,
            NormalizedImpact = normalizedImpact
        };

        // 9. Confidence scoring (optional)
        // Accepts pre-fetched outcome data — empty dict when no historical data is available.
        ConfidenceAssessment? confidence = null;
        if (command.IncludeConfidenceScoring)
        {
            var emptyOutcomes = new Dictionary<Guid, IReadOnlyList<SimulationOutcomeRecord>>();
            confidence = _confidenceScorer.Score(result, emptyOutcomes);
            result = result with { Confidence = confidence };
        }

        // 10. Drift detection (optional)
        // Accepts pre-fetched outcome records — empty list when no historical data is available.
        DriftAssessment? drift = null;
        if (command.IncludeDriftDetection && result.PolicyVersionsUsed.Count > 0)
        {
            drift = _driftDetector.Detect([]);
            result = result with { Drift = drift };
        }

        // 11. Build simulation outcome records for future calibration (caller persists)
        foreach (var policy in result.PolicyVersionsUsed)
        {
            _calibrationEngine.BuildSimulationOutcome(
                policy.PolicyId,
                policy.Version,
                decisions.OverallDecision,
                snapshot.SnapshotId);
        }

        return result;
    }

    private async Task<PolicySimulationResult> SimulateWithAggregationAsync(
        PolicySimulationCommand command,
        CancellationToken cancellationToken)
    {
        var aggregator = new SimulationAggregator(this);
        var singleRunCommand = command with { RunCount = 1 };

        var aggregation = await aggregator.AggregateAsync(
            singleRunCommand, command.RunCount, command.Seed, cancellationToken);

        // Run a canonical single simulation for the base result
        var baseResult = await SimulateAsync(singleRunCommand, cancellationToken);

        return baseResult with { Aggregation = aggregation };
    }

    public async Task<BatchSimulationResult> SimulateBatchAsync(
        BatchSimulationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Execute scenarios in parallel
        var tasks = command.Scenarios.Select(scenario =>
            SimulateAsync(scenario, cancellationToken));

        var results = await Task.WhenAll(tasks);

        // Detect cross-policy conflicts
        var conflicts = DetectCrossPolicyConflicts(results);

        return new BatchSimulationResult(results.ToList().AsReadOnly(), conflicts);
    }

    /// <summary>
    /// Returns quality metrics for the last simulation result.
    /// </summary>
    public SimulationQualityReport GetQualityMetrics(
        PolicySimulationResult result,
        CalibrationMetrics? calibration = null,
        DriftAssessment? drift = null)
    {
        return _qualityMetrics.Compute(result, calibration, drift ?? result.Drift);
    }

    private static string GenerateRecommendation(
        DecisionSummary decisions, RiskScore? risk, IReadOnlyList<SimulationAnomaly> anomalies)
    {
        if (anomalies.Any(a => a.Severity == "CRITICAL"))
            return "BLOCK — Critical anomalies detected. Do not activate without resolution.";

        if (risk is { Category: "CRITICAL" })
            return "BLOCK — Risk score is critical. Review all risk factors before activation.";

        if (risk is { Category: "HIGH" })
            return "REVIEW — High risk score. Governance review recommended before activation.";

        if (decisions.OverallDecision == "Deny" && decisions.Violations.Count > 3)
            return "REVIEW — Multiple violations detected. Policy may be overly restrictive.";

        if (decisions.OverallDecision == "Allow" && anomalies.Count == 0)
            return "PROCEED — Simulation passed with no anomalies.";

        return "REVIEW — Simulation completed. Manual review recommended.";
    }

    private static IReadOnlyList<PolicyConflictInteraction> DetectCrossPolicyConflicts(
        IReadOnlyList<PolicySimulationResult> results)
    {
        var conflicts = new List<PolicyConflictInteraction>();

        for (var i = 0; i < results.Count; i++)
        {
            for (var j = i + 1; j < results.Count; j++)
            {
                var a = results[i];
                var b = results[j];

                if (a.DecisionSummary.OverallDecision != b.DecisionSummary.OverallDecision)
                {
                    var policyAId = a.PolicyVersionsUsed.FirstOrDefault()?.PolicyId ?? Guid.Empty;
                    var policyBId = b.PolicyVersionsUsed.FirstOrDefault()?.PolicyId ?? Guid.Empty;

                    if (policyAId != Guid.Empty && policyBId != Guid.Empty)
                    {
                        conflicts.Add(new PolicyConflictInteraction(
                            policyAId, policyBId, "DecisionConflict",
                            $"Scenario {a.ScenarioId} decides {a.DecisionSummary.OverallDecision} " +
                            $"while scenario {b.ScenarioId} decides {b.DecisionSummary.OverallDecision}."));
                    }
                }
            }
        }

        return conflicts.AsReadOnly();
    }
}
