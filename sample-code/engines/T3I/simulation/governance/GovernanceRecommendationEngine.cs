using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Simulation;

/// <summary>
/// T3I engine: generates governance recommendations based on simulation results.
/// Outputs actionable recommendations: tighten, relax, require verification, require approval.
/// Stateless, deterministic. NEVER writes to chain or mutates state.
/// </summary>
public sealed class GovernanceRecommendationEngine : IEngine<GenerateRecommendationCommand>
{
    public Task<EngineResult> ExecuteAsync(
        GenerateRecommendationCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var actions = new List<RecommendedAction>();

        // Based on policy outcome
        if (command.PolicyOutcome is not null && !command.PolicyOutcome.Allowed)
        {
            actions.Add(new RecommendedAction(
                "review_policy",
                "Review policy rules that blocked this action",
                RecommendationPriority.Medium));
        }

        // Based on risk score
        if (command.RiskAssessment is not null)
        {
            switch (command.RiskAssessment.Category)
            {
                case "critical":
                    actions.Add(new RecommendedAction("require_approval",
                        "Require multi-party approval before execution", RecommendationPriority.Critical));
                    actions.Add(new RecommendedAction("tighten_policy",
                        "Tighten policy rules for this action category", RecommendationPriority.High));
                    break;
                case "high":
                    actions.Add(new RecommendedAction("require_verification",
                        "Require identity verification before execution", RecommendationPriority.High));
                    break;
                case "medium":
                    actions.Add(new RecommendedAction("monitor",
                        "Flag for monitoring — elevated risk", RecommendationPriority.Medium));
                    break;
            }
        }

        // Based on economic simulation
        if (command.EconomicResult is { ThresholdViolation: true })
        {
            actions.Add(new RecommendedAction("economic_review",
                $"Transaction exceeds threshold ({command.EconomicResult.Amount} {command.EconomicResult.Currency})",
                RecommendationPriority.High));
        }

        // Based on workflow simulation
        if (command.WorkflowResult is { HasBlockedPath: true })
        {
            actions.Add(new RecommendedAction("workflow_unblock",
                $"Workflow path blocked: {command.WorkflowResult.BlockReason}",
                RecommendationPriority.Medium));
        }

        // If everything clean — relax suggestion
        if (actions.Count == 0 && command.RiskAssessment?.Category == "low")
        {
            actions.Add(new RecommendedAction("relax_policy",
                "Low risk — consider relaxing policy constraints for efficiency",
                RecommendationPriority.Low));
        }

        var overallPriority = actions.Count > 0
            ? actions.Max(a => a.Priority)
            : RecommendationPriority.Low;

        var summary = actions.Count > 0
            ? $"{actions.Count} recommendation(s) generated. Highest priority: {overallPriority}"
            : "No governance actions required.";

        var result = new GovernanceRecommendation(
            Summary: summary,
            Actions: actions,
            OverallPriority: overallPriority,
            ActionCount: actions.Count);

        return Task.FromResult(EngineResult.Ok(result));
    }
}

public sealed record GenerateRecommendationCommand(
    PolicySimulationOutcome? PolicyOutcome,
    RiskAssessmentResult? RiskAssessment,
    EconomicSimulationResult? EconomicResult,
    WorkflowSimulationResult? WorkflowResult);

public sealed record GovernanceRecommendation(
    string Summary,
    List<RecommendedAction> Actions,
    RecommendationPriority OverallPriority,
    int ActionCount);

public sealed record RecommendedAction(
    string ActionType,
    string Description,
    RecommendationPriority Priority);

public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}
