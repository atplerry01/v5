namespace Whycespace.Systems.Midstream.Wss.Workflows.Governance.ClusterDecision;

/// <summary>
/// E18.7.4 — Declarative workflow definition for cluster governance decisions.
/// Systems layer: composition only, no execution.
///
/// Flow: Simulate (T3I) → Evaluate (T0U) → Approve → Execute
///       Evaluate DENY → Reject (compensation)
/// </summary>
public static class ClusterGovernanceWorkflowDefinition
{
    public const string WorkflowId = "CLUSTER_GOVERNANCE_DECISION_V1";

    public static readonly IReadOnlyList<string> Steps =
    [
        "SimulateGovernanceDecision",
        "EvaluateGovernancePolicy",
        "ApproveGovernanceDecision",
        "ExecuteGovernanceDecision"
    ];

    public static readonly IReadOnlyList<string> CompensationSteps =
    [
        "RejectGovernanceDecision"
    ];

    public static readonly IReadOnlyDictionary<string, string> Transitions =
        new Dictionary<string, string>
        {
            ["SimulateGovernanceDecision:SUCCESS"] = "EvaluateGovernancePolicy",
            ["EvaluateGovernancePolicy:ALLOW"] = "ApproveGovernanceDecision",
            ["EvaluateGovernancePolicy:DENY"] = "RejectGovernanceDecision",
            ["ApproveGovernanceDecision:SUCCESS"] = "ExecuteGovernanceDecision"
        };

    public static bool IsCompensation(string stepName) => CompensationSteps.Contains(stepName);

    public static string? GetNextStep(string currentStep, string outcome)
    {
        var key = $"{currentStep}:{outcome}";
        return Transitions.TryGetValue(key, out var next) ? next : null;
    }
}
