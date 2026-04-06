namespace Whycespace.Systems.Midstream.Wss.Workflows.Structural.Cluster.CrossSpv;

/// <summary>
/// E18.6.1 — Declarative workflow definition for cross-SPV execution.
/// Systems layer: composition only, no execution.
///
/// Flow: Prepare → Execute → Commit (or Compensate on failure)
/// </summary>
public static class CrossSpvWorkflowDefinition
{
    public const string WorkflowId = "CLUSTER_CROSSSPV_EXECUTION_V1";

    public static readonly IReadOnlyList<string> Steps =
    [
        "PrepareCrossSpvTransaction",
        "ExecuteCrossSpvLegs",
        "CommitCrossSpvTransaction"
    ];

    public static readonly IReadOnlyList<string> CompensationSteps =
    [
        "FailCrossSpvTransaction"
    ];

    public static readonly IReadOnlyDictionary<string, string> Transitions =
        new Dictionary<string, string>
        {
            ["PrepareCrossSpvTransaction:SUCCESS"] = "ExecuteCrossSpvLegs",
            ["ExecuteCrossSpvLegs:SUCCESS"] = "CommitCrossSpvTransaction",
            ["ExecuteCrossSpvLegs:FAILURE"] = "FailCrossSpvTransaction"
        };

    public static bool IsCompensation(string stepName) => CompensationSteps.Contains(stepName);

    public static string? GetNextStep(string currentStep, string outcome)
    {
        var key = $"{currentStep}:{outcome}";
        return Transitions.TryGetValue(key, out var next) ? next : null;
    }
}
