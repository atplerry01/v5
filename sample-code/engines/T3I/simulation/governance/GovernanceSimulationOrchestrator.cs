using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Utils;

namespace Whycespace.Engines.T3I.Simulation;

/// <summary>
/// T3I unified simulation orchestrator (E8).
/// Coordinates policy, economic, workflow, and risk simulations into a single result.
/// Stateless, deterministic, read-only. NEVER triggers execution, writes to chain, or mutates state.
///
/// Flow: SimulationCommand → Policy Sim → Economic Sim → Workflow Sim → Risk Score → Recommendation
/// </summary>
public sealed class GovernanceSimulationOrchestrator : IEngine<RunGovernanceSimulationCommand>
{
    private readonly EconomicSimulationEngine _economicSim = new();
    private readonly WorkflowSimulationEngine _workflowSim = new();
    private readonly RiskScoringEngine _riskEngine = new();
    private readonly GovernanceRecommendationEngine _recommendationEngine = new();

    public async Task<EngineResult> ExecuteAsync(
        RunGovernanceSimulationCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var simulationId = command.SimulationId ?? $"sim-{DeterministicIdHelper.FromSeed($"Simulation:{command.ScenarioType}:{command.SubjectId}:{command.Action}:{command.Resource}"):N}";

        // Phase 1: Simulate policy decision
        var policyResult = SimulatePolicy(command);

        // Phase 2: Simulate economic impact (if applicable)
        EconomicSimulationResult? economicResult = null;
        if (command.Amount.HasValue)
        {
            var econCmd = new SimulateEconomicImpactCommand(
                command.AccountId ?? "unknown",
                command.Amount.Value,
                command.Currency ?? "USD",
                command.Action,
                policyResult.Allowed);

            var econEngineResult = await _economicSim.ExecuteAsync(econCmd, context, cancellationToken);
            economicResult = econEngineResult.Data as EconomicSimulationResult;
        }

        // Phase 3: Simulate workflow path (if applicable)
        WorkflowSimulationResult? workflowResult = null;
        if (!string.IsNullOrWhiteSpace(command.WorkflowId))
        {
            var wfCmd = new SimulateWorkflowPathCommand(
                command.WorkflowId,
                command.StepId ?? "unknown",
                command.State ?? "created",
                command.Transition ?? "start",
                policyResult.Allowed);

            var wfEngineResult = await _workflowSim.ExecuteAsync(wfCmd, context, cancellationToken);
            workflowResult = wfEngineResult.Data as WorkflowSimulationResult;
        }

        // Phase 4: Compute risk score
        var riskCmd = new ComputeRiskScoreCommand(
            command.SubjectId,
            command.Action,
            command.TrustScore,
            command.Amount ?? 0m,
            command.Currency ?? "USD",
            policyResult.Allowed,
            economicResult?.ThresholdViolation ?? false,
            workflowResult?.HasBlockedPath ?? false);

        var riskEngineResult = await _riskEngine.ExecuteAsync(riskCmd, context, cancellationToken);
        var riskResult = riskEngineResult.Data as RiskAssessmentResult;

        // Phase 5: Generate governance recommendation
        var recCmd = new GenerateRecommendationCommand(
            policyResult,
            riskResult,
            economicResult,
            workflowResult);

        var recEngineResult = await _recommendationEngine.ExecuteAsync(recCmd, context, cancellationToken);
        var recommendation = recEngineResult.Data as GovernanceRecommendation;

        var result = new GovernanceSimulationResult
        {
            SimulationId = simulationId,
            ScenarioType = command.ScenarioType,
            PolicyOutcome = policyResult,
            EconomicImpact = economicResult,
            WorkflowPath = workflowResult,
            RiskAssessment = riskResult,
            Recommendation = recommendation,
            Deterministic = true
        };

        return EngineResult.Ok(result);
    }

    private static PolicySimulationOutcome SimulatePolicy(RunGovernanceSimulationCommand command)
    {
        // Deterministic policy simulation based on input parameters
        // In production, this delegates to PolicySimulationEngine via command dispatch
        var allowed = command.TrustScore >= 0.3 && !string.IsNullOrWhiteSpace(command.SubjectId);
        var predictedDecision = allowed ? "ALLOW" : "DENY";
        var reason = allowed ? null : "Simulated: trust score below threshold or missing identity";

        return new PolicySimulationOutcome(
            Allowed: allowed,
            PredictedDecision: predictedDecision,
            DenialReason: reason,
            PolicyId: command.PolicyId ?? "default");
    }
}

public sealed record RunGovernanceSimulationCommand
{
    public string? SimulationId { get; init; }
    public required string ScenarioType { get; init; }
    public required string SubjectId { get; init; }
    public required string Action { get; init; }
    public required string Resource { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? WorkflowId { get; init; }
    public string? StepId { get; init; }
    public string? State { get; init; }
    public string? Transition { get; init; }
    public string? AccountId { get; init; }
    public string? PolicyId { get; init; }
    public double TrustScore { get; init; }
    public string[] Roles { get; init; } = [];
}

public sealed record GovernanceSimulationResult
{
    public required string SimulationId { get; init; }
    public required string ScenarioType { get; init; }
    public required PolicySimulationOutcome PolicyOutcome { get; init; }
    public EconomicSimulationResult? EconomicImpact { get; init; }
    public WorkflowSimulationResult? WorkflowPath { get; init; }
    public RiskAssessmentResult? RiskAssessment { get; init; }
    public GovernanceRecommendation? Recommendation { get; init; }
    public bool Deterministic { get; init; }
}

public sealed record PolicySimulationOutcome(
    bool Allowed,
    string PredictedDecision,
    string? DenialReason,
    string PolicyId);
