namespace Whycespace.Engines.T3I.Simulation.Governance;

/// <summary>
/// Autonomous Governance Loop: Detect → Simulate → Score → Propose.
/// Stateless engine that produces governance proposals from detected conditions.
/// Approval and execution happen in domain and runtime respectively.
/// </summary>
public sealed class GovernanceLoopEngine
{
    private readonly GovernanceSimulationOrchestrator _simulator;
    private readonly GovernanceRecommendationEngine _recommender;

    public GovernanceLoopEngine(
        GovernanceSimulationOrchestrator simulator,
        GovernanceRecommendationEngine recommender)
    {
        _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        _recommender = recommender ?? throw new ArgumentNullException(nameof(recommender));
    }

    public GovernanceLoopResult Execute(GovernanceLoopCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Phase 1: DETECT — evaluate current system state against thresholds
        var detections = Detect(command);
        if (detections.Count == 0)
            return GovernanceLoopResult.NoAction("No governance issues detected");

        // Phase 2: SIMULATE — run governance simulation for each detection
        var simulationResults = new List<SimulationOutcome>();
        foreach (var detection in detections)
        {
            var outcome = Simulate(detection, command);
            simulationResults.Add(outcome);
        }

        // Phase 2.5: SCORE — assess risk and economic impact for each simulation
        var scoredResults = new List<ScoredSimulation>();
        foreach (var sim in simulationResults.Where(s => s.RequiresAction))
        {
            var riskScore = AssessRisk(sim, command);
            var economicImpact = EstimateEconomicImpact(sim, command);
            scoredResults.Add(new ScoredSimulation(sim, riskScore, economicImpact));
        }

        // Phase 3: PROPOSE — generate proposals from scored simulation results
        var proposals = new List<GovernanceProposal>();
        foreach (var scored in scoredResults)
        {
            proposals.Add(new GovernanceProposal(
                DetectionType: scored.Outcome.DetectionType,
                Summary: scored.Outcome.Summary,
                RecommendedAction: scored.Outcome.RecommendedAction,
                Priority: scored.Outcome.Priority,
                SimulationConfidence: scored.Outcome.Confidence,
                RequiresQuorum: scored.Outcome.Priority is "Critical" or "High",
                Risk: scored.Risk,
                Impact: scored.Impact));
        }

        return new GovernanceLoopResult(
            Detections: detections,
            Proposals: proposals,
            RequiresApproval: proposals.Any(p => p.RequiresQuorum));
    }

    private static List<GovernanceDetection> Detect(GovernanceLoopCommand command)
    {
        var detections = new List<GovernanceDetection>();

        // Policy drift detection
        if (command.PolicyViolationCount > command.PolicyViolationThreshold)
        {
            detections.Add(new GovernanceDetection(
                DetectionTypes.PolicyDrift,
                $"Policy violations ({command.PolicyViolationCount}) exceed threshold ({command.PolicyViolationThreshold})",
                "High"));
        }

        // Revenue anomaly detection
        if (command.RevenueDeviationPercent > 25m)
        {
            detections.Add(new GovernanceDetection(
                DetectionTypes.RevenueAnomaly,
                $"Revenue deviation at {command.RevenueDeviationPercent:F1}% — exceeds 25% threshold",
                command.RevenueDeviationPercent > 50m ? "Critical" : "High"));
        }

        // Workflow failure rate
        if (command.WorkflowFailureRate > 0.1m)
        {
            detections.Add(new GovernanceDetection(
                DetectionTypes.WorkflowDegradation,
                $"Workflow failure rate at {command.WorkflowFailureRate:P0} — exceeds 10% threshold",
                command.WorkflowFailureRate > 0.3m ? "Critical" : "Medium"));
        }

        // Capital utilization concern
        if (command.CapitalUtilizationRate > 0.9m)
        {
            detections.Add(new GovernanceDetection(
                DetectionTypes.CapitalStress,
                $"Capital utilization at {command.CapitalUtilizationRate:P0} — approaching limit",
                "High"));
        }

        return detections;
    }

    private static SimulationOutcome Simulate(GovernanceDetection detection, GovernanceLoopCommand command)
    {
        var (action, confidence) = detection.Type switch
        {
            DetectionTypes.PolicyDrift => ("Tighten policy enforcement and trigger compliance review", 0.85m),
            DetectionTypes.RevenueAnomaly => ("Initiate economic review and suspend affected operations", 0.8m),
            DetectionTypes.WorkflowDegradation => ("Route workflows to fallback path and alert operations", 0.75m),
            DetectionTypes.CapitalStress => ("Restrict new capital commitments and optimize allocations", 0.9m),
            _ => ("Monitor and reassess", 0.5m)
        };

        return new SimulationOutcome(
            DetectionType: detection.Type,
            Summary: detection.Description,
            RecommendedAction: action,
            Priority: detection.Priority,
            Confidence: confidence,
            RequiresAction: true);
    }

    private static RiskAssessment AssessRisk(SimulationOutcome sim, GovernanceLoopCommand command)
    {
        var baseRisk = sim.Priority switch
        {
            "Critical" => 0.9m,
            "High" => 0.7m,
            "Medium" => 0.4m,
            _ => 0.2m
        };

        // Compound risk: multiple detections increase overall risk
        var compoundFactor = 1.0m + (command.PolicyViolationCount > 0 ? 0.1m : 0m)
            + (command.RevenueDeviationPercent > 25m ? 0.15m : 0m)
            + (command.WorkflowFailureRate > 0.1m ? 0.1m : 0m)
            + (command.CapitalUtilizationRate > 0.9m ? 0.15m : 0m);

        var adjustedRisk = Math.Min(1.0m, baseRisk * compoundFactor);

        var category = adjustedRisk switch
        {
            >= 0.8m => "Systemic",
            >= 0.6m => "Operational",
            >= 0.3m => "Localized",
            _ => "Minimal"
        };

        return new RiskAssessment(adjustedRisk, category, sim.DetectionType);
    }

    private static EconomicImpact EstimateEconomicImpact(SimulationOutcome sim, GovernanceLoopCommand command)
    {
        var (estimatedLoss, mitigationBenefit) = sim.DetectionType switch
        {
            "POLICY_DRIFT" => (command.PolicyViolationCount * 1000m, command.PolicyViolationCount * 800m),
            "REVENUE_ANOMALY" => (command.RevenueDeviationPercent * 500m, command.RevenueDeviationPercent * 350m),
            "WORKFLOW_DEGRADATION" => (command.WorkflowFailureRate * 50000m, command.WorkflowFailureRate * 40000m),
            "CAPITAL_STRESS" => (command.CapitalUtilizationRate * 100000m, command.CapitalUtilizationRate * 75000m),
            _ => (0m, 0m)
        };

        return new EconomicImpact(estimatedLoss, mitigationBenefit, mitigationBenefit - estimatedLoss * 0.1m);
    }

    private static class DetectionTypes
    {
        public const string PolicyDrift = "POLICY_DRIFT";
        public const string RevenueAnomaly = "REVENUE_ANOMALY";
        public const string WorkflowDegradation = "WORKFLOW_DEGRADATION";
        public const string CapitalStress = "CAPITAL_STRESS";
    }
}

// Commands and results
public sealed record GovernanceLoopCommand(
    int PolicyViolationCount,
    int PolicyViolationThreshold,
    decimal RevenueDeviationPercent,
    decimal WorkflowFailureRate,
    decimal CapitalUtilizationRate);

public sealed record GovernanceLoopResult(
    IReadOnlyList<GovernanceDetection> Detections,
    IReadOnlyList<GovernanceProposal> Proposals,
    bool RequiresApproval)
{
    public static GovernanceLoopResult NoAction(string reason)
        => new([], [], false);
}

public sealed record GovernanceDetection(string Type, string Description, string Priority);

public sealed record SimulationOutcome(
    string DetectionType,
    string Summary,
    string RecommendedAction,
    string Priority,
    decimal Confidence,
    bool RequiresAction);

public sealed record GovernanceProposal(
    string DetectionType,
    string Summary,
    string RecommendedAction,
    string Priority,
    decimal SimulationConfidence,
    bool RequiresQuorum,
    RiskAssessment Risk,
    EconomicImpact Impact);

public sealed record ScoredSimulation(
    SimulationOutcome Outcome,
    RiskAssessment Risk,
    EconomicImpact Impact);

public sealed record RiskAssessment(
    decimal Score,
    string Category,
    string Source);

public sealed record EconomicImpact(
    decimal EstimatedLoss,
    decimal MitigationBenefit,
    decimal NetBenefit);
